//
//  ScanViewController.swift
//  HomeVision
//
//  Created by Ethan on 2022/1/17.
//

import RealityKit
import ARKit

class MyARView: ARView, ARSessionDelegate {
    
    var modelsForClassification: [ARMeshClassification: ModelEntity] = [:]
    var viewModel: MyViewModel!
    
    init(frame: CGRect, viewModel: MyViewModel) {
        super.init(frame: frame)
        
        self.viewModel = viewModel
        
        self.environment.sceneUnderstanding.options = []
        
        // Turn on occlusion from the scene reconstruction's mesh.
        self.environment.sceneUnderstanding.options.insert(.occlusion)
        
        // Turn on physics for the scene reconstruction's mesh.
        self.environment.sceneUnderstanding.options.insert(.physics)

        // Display a debug visualization of the mesh.
        self.debugOptions.insert(.showSceneUnderstanding)
        
        // For performance, disable render options that are not required for this app.
        self.renderOptions = [.disablePersonOcclusion, .disableDepthOfField, .disableMotionBlur]
        
        // Manually configure what kind of AR session to run since
        // ARView on its own does not turn on mesh classification.
        self.automaticallyConfigureSession = false
        let configuration = ARWorldTrackingConfiguration()
        configuration.sceneReconstruction = .meshWithClassification

        configuration.environmentTexturing = .automatic
        self.session.run(configuration)
        
        let tapRecognizer = UITapGestureRecognizer(target: self, action: #selector(handleTap(_:)))
        self.addGestureRecognizer(tapRecognizer)
    }
    
    required init?(coder decoder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
    
    required init(frame frameRect: CGRect) {
        fatalError("init(frame:) has not been implemented")
    }
    
    
    private var dot_anchor:[AnchorEntity] = []
    
    @objc
    func handleTap(_ sender: UITapGestureRecognizer) {
        print("tapped")
        // 1. Perform a ray cast against the mesh.
        // Note: Ray-cast option ".estimatedPlane" with alignment ".any" also takes the mesh into account.
        let tapLocation = sender.location(in: self)
        if let result = self.raycast(from: tapLocation, allowing: .estimatedPlane, alignment: .any).first {
            // ...
            // 2. Visualize the intersection point of the ray with the real-world surface.
            let resultAnchor = AnchorEntity(world: result.worldTransform)
            resultAnchor.addChild(sphere(radius: 0.01, color: .lightGray))
            self.scene.addAnchor(resultAnchor)
            dot_anchor.append(resultAnchor)
            
            print(result.worldTransform.position)//ray hit location

            
            // 3. Try to get a classification near the tap location.
            //    Classifications are available per face (in the geometric sense, not human faces).
//            nearbyFaceWithClassification(to: result.worldTransform.position) { (centerOfFace, classification) in
//                // ...
//                DispatchQueue.main.async {
//                    // 4. Compute a position for the text which is near the result location, but offset 10 cm
//                    // towards the camera (along the ray) to minimize unintentional occlusions of the text by the mesh.
//                    let rayDirection = normalize(result.worldTransform.position - self.cameraTransform.translation)
//                    let textPositionInWorldCoordinates = result.worldTransform.position - (rayDirection * 0.1)
//
//                    // 5. Create a 3D text to visualize the classification result.
//                    let textEntity = self.model(for: classification)
//
//                    // 6. Scale the text depending on the distance, such that it always appears with
//                    //    the same size on screen.
//                    let raycastDistance = distance(result.worldTransform.position, self.cameraTransform.translation)
//                    textEntity.scale = .one * raycastDistance
//
//                    // 7. Place the text, facing the camera.
//                    var resultWithCameraOrientation = self.cameraTransform
//                    resultWithCameraOrientation.translation = textPositionInWorldCoordinates
//                    let textAnchor = AnchorEntity(world: resultWithCameraOrientation.matrix)
//                    textAnchor.addChild(textEntity)
//                    self.scene.addAnchor(textAnchor)
//                }
//            }
        }
    }
    
    func clearAnchorObjects(){        
        if(!dot_anchor.isEmpty){
            for obj in dot_anchor{
                self.scene.removeAnchor(obj)
            }
            
            dot_anchor.removeAll()
        }
        viewModel.ClearAnchorObjects = false
    }
    
    func nearbyFaceWithClassification(to location: SIMD3<Float>, completionBlock: @escaping (SIMD3<Float>?, ARMeshClassification) -> Void) {
        guard let frame = self.session.currentFrame else {
            completionBlock(nil, .none)
            return
        }
    
        var meshAnchors = frame.anchors.compactMap({ $0 as? ARMeshAnchor })
        
        // Sort the mesh anchors by distance to the given location and filter out
        // any anchors that are too far away (4 meters is a safe upper limit).
        let cutoffDistance: Float = 4.0
        meshAnchors.removeAll { distance($0.transform.position, location) > cutoffDistance }
        meshAnchors.sort { distance($0.transform.position, location) < distance($1.transform.position, location) }

        // Perform the search asynchronously in order not to stall rendering.
        DispatchQueue.global().async {
            for anchor in meshAnchors {
                for index in 0..<anchor.geometry.faces.count {
                    // Get the center of the face so that we can compare it to the given location.
                    let geometricCenterOfFace = anchor.geometry.centerOf(faceWithIndex: index)
                    
                    // Convert the face's center to world coordinates.
                    var centerLocalTransform = matrix_identity_float4x4
                    centerLocalTransform.columns.3 = SIMD4<Float>(geometricCenterOfFace.0, geometricCenterOfFace.1, geometricCenterOfFace.2, 1)
                    let centerWorldPosition = (anchor.transform * centerLocalTransform).position
                     
                    // We're interested in a classification that is sufficiently close to the given location––within 5 cm.
                    let distanceToFace = distance(centerWorldPosition, location)
                    if distanceToFace <= 0.05 {
                        // Get the semantic classification of the face and finish the search.
                        let classification: ARMeshClassification = anchor.geometry.classificationOf(faceWithIndex: index)
                        completionBlock(centerWorldPosition, classification)
                        return
                    }
                }
            }
            
            // Let the completion block know that no result was found.
            completionBlock(nil, .none)
        }
    }
    
    func model(for classification: ARMeshClassification) -> ModelEntity {
        // Return cached model if available
        if let model = modelsForClassification[classification] {
            model.transform = .identity
            return model.clone(recursive: true)
        }
        
        // Generate 3D text for the classification
        let lineHeight: CGFloat = 0.05
        let font = MeshResource.Font.systemFont(ofSize: lineHeight)
        let textMesh = MeshResource.generateText(classification.description, extrusionDepth: Float(lineHeight * 0.1), font: font)
        let textMaterial = SimpleMaterial(color: classification.color, isMetallic: true)
        let model = ModelEntity(mesh: textMesh, materials: [textMaterial])
        // Move text geometry to the left so that its local origin is in the center
        model.position.x -= model.visualBounds(relativeTo: nil).extents.x / 2
        // Add model to cache
        modelsForClassification[classification] = model
        return model
    }
    
    func sphere(radius: Float, color: UIColor) -> ModelEntity {
        let sphere = ModelEntity(mesh: .generateSphere(radius: radius), materials: [SimpleMaterial(color: color, isMetallic: false)])
        // Move sphere up by half its diameter so that it does not intersect with the mesh
        sphere.position.y = radius
        return sphere
    }
    
}
