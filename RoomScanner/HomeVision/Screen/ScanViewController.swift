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
    var objModel: PlacingObjectModel!
    
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
    
    
    private var dot_anchor_list:[AnchorEntity] = []
    private var text_anchor_list:[AnchorEntity] = []
    
    @objc
    func handleTap(_ sender: UITapGestureRecognizer) {
        print("tapped")
        // Perform a ray cast against the mesh.
        let tapLocation = sender.location(in: self)
        if let result = self.raycast(from: tapLocation, allowing: .estimatedPlane, alignment: .any).first {
            // Visualize the intersection point of the ray with the real-world surface.
            let dotAnchor = AnchorEntity(world: result.worldTransform)
            dotAnchor.addChild(sphere(radius: 0.02, color: .lightGray))
            self.scene.addAnchor(dotAnchor)
            dot_anchor_list.append(dotAnchor)
            
            
            // Create a 3D text to visualize the classification result.
            let rayDirection = normalize(result.worldTransform.position - self.cameraTransform.translation)
            let textPositionInWorldCoordinates = result.worldTransform.position - (rayDirection * 0.1)
            // Create a 3D text to visualize the classification result.
            let textEntity = self.textModel(text: objModel.objType.rawValue)

            // Scale the text depending on the distance, such that it always appears with the same size on screen.
            let raycastDistance = distance(result.worldTransform.position, self.cameraTransform.translation)
            textEntity.scale = .one * raycastDistance

            // Place the text, facing the camera.
            var resultWithCameraOrientation = self.cameraTransform
            resultWithCameraOrientation.translation = textPositionInWorldCoordinates
            let textAnchor = AnchorEntity(world: resultWithCameraOrientation.matrix)
            textAnchor.addChild(textEntity)
            self.scene.addAnchor(textAnchor)
            text_anchor_list.append(textAnchor)
            
            print(result.worldTransform.position)//ray hit location

        }
    }
    
    func clearAnchorObjects(){        
        if(!dot_anchor_list.isEmpty){
            for obj in dot_anchor_list{
                self.scene.removeAnchor(obj)
            }
            
            dot_anchor_list.removeAll()
        }
        
        if(!text_anchor_list.isEmpty){
            for textObj in text_anchor_list{
                self.scene.removeAnchor(textObj)
            }
            
            text_anchor_list.removeAll()
        }
        
        viewModel.ClearAnchorObjects = false
    }

    
    func textModel(text: String) -> ModelEntity {
        // Generate 3D text for the Object
        let lineHeight: CGFloat = 0.05
        let font = MeshResource.Font.systemFont(ofSize: lineHeight)
        let textMesh = MeshResource.generateText(text, extrusionDepth: Float(lineHeight * 0.1), font: font)
        let textMaterial = SimpleMaterial(color: .lightGray, isMetallic: true)
        let model = ModelEntity(mesh: textMesh, materials: [textMaterial])
        // Move text geometry to the left so that its local origin is in the center
        model.position.x -= model.visualBounds(relativeTo: nil).extents.x / 2
        return model
    }
    
    func sphere(radius: Float, color: UIColor) -> ModelEntity {
        let sphere = ModelEntity(mesh: .generateSphere(radius: radius), materials: [SimpleMaterial(color: color, isMetallic: false)])
        // Move sphere up by half its diameter so that it does not intersect with the mesh
        sphere.position.y = radius
        return sphere
    }
    
}
