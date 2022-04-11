//
//  ScanViewController.swift
//  POSE
//
//  Created by Ethan on 2022/3/30.
//

import RealityKit
import ARKit
import UIKit
import MetalKit

class MyARView: ARView, ARSessionDelegate {
    
    var modelsForClassification: [ARMeshClassification: ModelEntity] = [:]
    var viewModel: MyViewModel!
    var objModel: PlacingObjectModel!
    var arView: ARView!
    
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
        self.session.delegate = self
        let configuration = ARWorldTrackingConfiguration()
        configuration.worldAlignment = .gravity
        configuration.planeDetection = [.vertical, .horizontal]
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
    private var obj_name_list:[String] = []
    private var obj_pos_list:[SIMD3<Float>] = []
    private var text_anchor_list:[AnchorEntity] = []
    private var obj_cnt = 0
//    private var plane_cood_list:[String] = []

    @objc
    func handleTap(_ sender: UITapGestureRecognizer) {
        // Perform a ray cast against the mesh.
        let tapLocation = sender.location(in: self)
        if let result = self.raycast(from: tapLocation, allowing: .estimatedPlane, alignment: .any).first {
            // Visualize the intersection point of the ray with the real-world surface.
            let dotAnchor = AnchorEntity(world: result.worldTransform)
            dotAnchor.addChild(sphere(radius: 0.02, color: .lightGray))
            self.scene.addAnchor(dotAnchor)
            dot_anchor_list.append(dotAnchor)
            obj_name_list.append(objModel.objType.rawValue + String(obj_cnt))
            obj_pos_list.append(result.worldTransform.position)
            obj_cnt += 1
            
            // Create a 3D text to visualize the classification result.
            let rayDirection = normalize(result.worldTransform.position - self.cameraTransform.translation)
//            let textPositionInWorldCoordinates = result.worldTransform.position - (rayDirection * 0.1)
            let textPositionInWorldCoordinates = result.worldTransform.position - (rayDirection * Float(0.1))
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
        }
    }
    
//    ********************************Plane Detection********************************
    
//    func session(_ session: ARSession, didAdd anchors: [ARAnchor]) {
//        var send_str: String = ""
//        for anchor in anchors {
//            if anchor is ARPlaneAnchor {
//                let planeAnchor = anchor as! ARPlaneAnchor
//                print("added plane")
//                addPlaneEntity(with: planeAnchor)
////                let newPlaneCoord = PlaneCoord()
////                newPlaneCoord.pointTopLeft = SIMD3.init(planeAnchor.worldPoints().0)
////                newPlaneCoord.pointTopRight = SIMD3.init(planeAnchor.worldPoints().1)
////                newPlaneCoord.pointBottomLeft = SIMD3.init(planeAnchor.worldPoints().2)
////                newPlaneCoord.pointBottomRight = SIMD3.init(planeAnchor.worldPoints().3)
//                for cood in planeAnchor.worldPoints(){
//                    send_str += "("+cood.x.description+", "+cood.y.description+", "+cood.z.description+")"
//                }
////                plane_cood_list.append(send_str)
//            }
//        }
//    }
//
//    func session(_ session: ARSession, didUpdate anchors: [ARAnchor]) {
//        for anchor in anchors {
//            if anchor is ARPlaneAnchor {
//                let planeAnchor = anchor as! ARPlaneAnchor
//                updatePlaneEntity(with: planeAnchor)
//            }
//        }
//    }
//
//    func session(_ session: ARSession, didRemove anchors: [ARAnchor]) {
//        for anchor in anchors {
//            if anchor is ARPlaneAnchor {
//                let planeAnchor = anchor as! ARPlaneAnchor
//                removePlaneEntity(with: planeAnchor)
//            }
//        }
//    }
//
//    func addPlaneEntity(with anchor: ARPlaneAnchor) {
//
//        let planeAnchorEntity = AnchorEntity(.plane([.any],
//                                        classification: [.any],
//                                        minimumBounds: [0.5, 0.5]))
//        let planeModelEntity = createPlaneModelEntity(with: anchor)
//
//        planeAnchorEntity.name = anchor.identifier.uuidString + "_anchor"
//        planeModelEntity.name = anchor.identifier.uuidString + "_model"
//
//        planeAnchorEntity.addChild(planeModelEntity)
//
//        self.scene.addAnchor(planeAnchorEntity)
//
//    }
//
//    func createPlaneModelEntity(with anchor: ARPlaneAnchor) -> ModelEntity {
//        var planeMesh: MeshResource
//        var color: UIColor
//
//        if anchor.alignment == .horizontal {
//            print("horizotal plane")
//            color = UIColor.blue.withAlphaComponent(0.5)
//            planeMesh = .generatePlane(width: anchor.extent.x, depth: anchor.extent.z)
//        } else if anchor.alignment == .vertical {
//            print("vertical plane")
//            color = UIColor.yellow.withAlphaComponent(0.5)
//            planeMesh = .generatePlane(width: anchor.extent.x, height: anchor.extent.z)
//        } else {
//            fatalError("Anchor is not ARPlaneAnchor")
//        }
//
//        return ModelEntity(mesh: planeMesh, materials: [SimpleMaterial(color: color, roughness: 0.25, isMetallic: false)])
//    }
//
//    func removePlaneEntity(with anchor: ARPlaneAnchor) {
//        guard let planeAnchorEntity = self.scene.findEntity(named: anchor.identifier.uuidString+"_anchor") else { return }
//        self.scene.removeAnchor(planeAnchorEntity as! AnchorEntity)
//    }
//
//    func updatePlaneEntity(with anchor: ARPlaneAnchor) {
//        var planeMesh: MeshResource
//        guard let entity = self.scene.findEntity(named: anchor.identifier.uuidString+"_model") else { return }
//        let modelEntity = entity as! ModelEntity
//
//        if anchor.alignment == .horizontal {
//            planeMesh = .generatePlane(width: anchor.extent.x, depth: anchor.extent.z)
//        } else if anchor.alignment == .vertical {
//            planeMesh = .generatePlane(width: anchor.extent.x, height: anchor.extent.z)
//        } else {
//            fatalError("Anchor is not ARPlaneAnchor")
//        }
//
//        modelEntity.model!.mesh = planeMesh
//    }
    
//    ************************************************************************************
    
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
        
        obj_pos_list.removeAll()
        obj_name_list.removeAll()
        obj_cnt = 0
        
        viewModel.ClearAnchorObjects = false
    }
    
    func donePressed(){
        var contain_camera = false
        if(!obj_name_list.isEmpty && !obj_pos_list.isEmpty){
            //check if data contains camera
            for obj in obj_name_list{
                if (obj.contains("camera")){
                    contain_camera = true
                }
            }
            
            if(!contain_camera){
                print("Error, No camera selected")
                viewModel.PopError = ErrorInfo(id: 1, title: "Error", description: "Please select at least one camera")
                viewModel.DonePressed = false
                return
            }else{
                var data_send = [String: String]()
                for(index, _) in obj_name_list.enumerated(){
                    data_send[obj_name_list[index]] = obj_pos_list[index].description.dropFirst(12).description
                }
                restApi(data: data_send)
            }
        }else{
            viewModel.PopError = ErrorInfo(id: 2, title: "Error", description: "No device is selected")
        }
        
        viewModel.DonePressed = false
        
//        DispatchQueue.background(background: {self.exportMesh()})
        exportMesh()
    }
    
    func exportMesh(){
        guard let frame = self.session.currentFrame else {
            fatalError("Couldn't get the current ARFrame")
        }
        
        // Fetch the default MTLDevice to initialize a MetalKit buffer allocator with
        guard let device = MTLCreateSystemDefaultDevice() else {
            fatalError("Failed to get the system's default Metal device!")
        }
        
        // Using the Model I/O framework to export the scan, so we're initialising an MDLAsset object,
        // which we can export to a file later, with a buffer allocator
        let allocator = MTKMeshBufferAllocator(device: device)
        let asset = MDLAsset(bufferAllocator: allocator)
        
        // Fetch all ARMeshAncors
        let meshAnchors = frame.anchors.compactMap({ $0 as? ARMeshAnchor })
        
        // Convert the geometry of each ARMeshAnchor into a MDLMesh and add it to the MDLAsset
        for meshAncor in meshAnchors {
            
            // Some short handles, otherwise stuff will get pretty long in a few lines
            let geometry = meshAncor.geometry
            let vertices = geometry.vertices
            let faces = geometry.faces
            let verticesPointer = vertices.buffer.contents()
            let facesPointer = faces.buffer.contents()
            
            // Converting each vertex of the geometry from the local space of their ARMeshAnchor to world space
            for vertexIndex in 0..<vertices.count {
                
                // Extracting the current vertex with an extension method provided by Apple in Extensions.swift
                let vertex = geometry.vertex(at: UInt32(vertexIndex))
                
                // Building a transform matrix with only the vertex position
                // and apply the mesh anchors transform to convert into world space
                var vertexLocalTransform = matrix_identity_float4x4
                vertexLocalTransform.columns.3 = SIMD4<Float>(x: vertex.0, y: vertex.1, z: vertex.2, w: 1)
                let vertexWorldPosition = (meshAncor.transform * vertexLocalTransform).position
                
                // Writing the world space vertex back into it's position in the vertex buffer
                let vertexOffset = vertices.offset + vertices.stride * vertexIndex
                let componentStride = vertices.stride / 3
                verticesPointer.storeBytes(of: vertexWorldPosition.x, toByteOffset: vertexOffset, as: Float.self)
                verticesPointer.storeBytes(of: vertexWorldPosition.y, toByteOffset: vertexOffset + componentStride, as: Float.self)
                verticesPointer.storeBytes(of: vertexWorldPosition.z, toByteOffset: vertexOffset + (2 * componentStride), as: Float.self)
            }
            
            // Initializing MDLMeshBuffers with the content of the vertex and face MTLBuffers
            let byteCountVertices = vertices.count * vertices.stride
            let byteCountFaces = faces.count * faces.indexCountPerPrimitive * faces.bytesPerIndex
            let vertexBuffer = allocator.newBuffer(with: Data(bytesNoCopy: verticesPointer, count: byteCountVertices, deallocator: .none), type: .vertex)
            let indexBuffer = allocator.newBuffer(with: Data(bytesNoCopy: facesPointer, count: byteCountFaces, deallocator: .none), type: .index)
            
            // Creating a MDLSubMesh with the index buffer and a generic material
            let indexCount = faces.count * faces.indexCountPerPrimitive
            let material = MDLMaterial(name: "mat1", scatteringFunction: MDLPhysicallyPlausibleScatteringFunction())
            let submesh = MDLSubmesh(indexBuffer: indexBuffer, indexCount: indexCount, indexType: .uInt32, geometryType: .triangles, material: material)
            
            // Creating a MDLVertexDescriptor to describe the memory layout of the mesh
            let vertexFormat = MTKModelIOVertexFormatFromMetal(vertices.format)
            let vertexDescriptor = MDLVertexDescriptor()
            vertexDescriptor.attributes[0] = MDLVertexAttribute(name: MDLVertexAttributePosition, format: vertexFormat, offset: 0, bufferIndex: 0)
            vertexDescriptor.layouts[0] = MDLVertexBufferLayout(stride: meshAncor.geometry.vertices.stride)
            
            // Finally creating the MDLMesh and adding it to the MDLAsset
            let mesh = MDLMesh(vertexBuffer: vertexBuffer, vertexCount: meshAncor.geometry.vertices.count, descriptor: vertexDescriptor, submeshes: [submesh])
            asset.add(mesh)
        }
        
        // Setting the path to export the OBJ file to
        let documentsPath = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask).first!
        let urlOBJ = documentsPath.appendingPathComponent("room.obj")
        viewModel.ObjFilePath = urlOBJ
        print(viewModel.ObjFilePath)

        // Exporting the OBJ file
        if MDLAsset.canExportFileExtension("obj") {
            do {
                try asset.export(to: urlOBJ)
            } catch let error {
                fatalError(error.localizedDescription)
            }
        } else {
            fatalError("Can't export OBJ")
        }
        
        
    }
    
    func restApi(data: Dictionary<String, String>){
        if(!data.isEmpty){
            let params = data as Dictionary<String, String>
            let url = URL(string: "http://192.168.2.53:3000/server")!
            var request = URLRequest(url: url)
            request.httpMethod = "POST"
            request.httpBody = try? JSONSerialization.data(withJSONObject: params, options: [])
            request.addValue("application/json", forHTTPHeaderField: "Content-Type")


            let task = URLSession.shared.dataTask(with: request) { data, response, error in
                guard let data = data,
                    let response = response as? HTTPURLResponse,
                    error == nil else {                                              // check for fundamental networking error
                    print("error", error ?? "Unknown error")
                    self.viewModel.PopError = ErrorInfo(id: 3, title: "Error", description: "Network Error")
                    return
                }

                guard (200 ... 299) ~= response.statusCode else {                    // check for http errors
                    print("statusCode should be 2xx, but is \(response.statusCode)")
                    print("response = \(response)")
                    self.viewModel.PopError = ErrorInfo(id: 3, title: "Error", description: "Network Error")
                    return
                }

                let responseString = String(data: data, encoding: .utf8)
                print("responseString = \(responseString)")
                self.viewModel.PopError = ErrorInfo(id: 4, title: "Successful", description: "Data sent successfully to the server")
            }

            task.resume()
        }
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

