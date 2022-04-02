//
//  Extension.swift
//  POSE
//
//  Created by Ethan on 2022/3/30.
//

import ARKit
import RealityKit
import MetalKit

extension simd_float4x4 {
    var position: SIMD3<Float> {
        return SIMD3<Float>(columns.3.x, columns.3.y, columns.3.z)
    }
}

extension ARMeshClassification {
    var description: String {
        switch self {
        case .ceiling: return "Ceiling"
        case .door: return "Door"
        case .floor: return "Floor"
        case .seat: return "Seat"
        case .table: return "Table"
        case .wall: return "Wall"
        case .window: return "Window"
        case .none: return "None"
        @unknown default: return "Unknown"
        }
    }
    
    var color: UIColor {
        switch self {
        case .ceiling: return .cyan
        case .door: return .brown
        case .floor: return .red
        case .seat: return .purple
        case .table: return .yellow
        case .wall: return .green
        case .window: return .blue
        case .none: return .lightGray
        @unknown default: return .gray
        }
    }
}

extension Transform {
    static func * (left: Transform, right: Transform) -> Transform {
        return Transform(matrix: simd_mul(left.matrix, right.matrix))
    }
}

extension ARMeshGeometry {
    func vertex(at index: UInt32) -> (Float, Float, Float) {
        assert(vertices.format == MTLVertexFormat.float3, "Expected three floats (twelve bytes) per vertex.")
        let vertexPointer = vertices.buffer.contents().advanced(by: vertices.offset + (vertices.stride * Int(index)))
        let vertex = vertexPointer.assumingMemoryBound(to: (Float, Float, Float).self).pointee
        return vertex
    }
    
    /// To get the mesh's classification, the sample app parses the classification's raw data and instantiates an
    /// `ARMeshClassification` object. For efficiency, ARKit stores classifications in a Metal buffer in `ARMeshGeometry`.
    func classificationOf(faceWithIndex index: Int) -> ARMeshClassification {
        guard let classification = classification else { return .none }
        assert(classification.format == MTLVertexFormat.uchar, "Expected one unsigned char (one byte) per classification")
        let classificationPointer = classification.buffer.contents().advanced(by: classification.offset + (classification.stride * index))
        let classificationValue = Int(classificationPointer.assumingMemoryBound(to: CUnsignedChar.self).pointee)
        return ARMeshClassification(rawValue: classificationValue) ?? .none
    }
    
    func vertexIndicesOf(faceWithIndex faceIndex: Int) -> [UInt32] {
        assert(faces.bytesPerIndex == MemoryLayout<UInt32>.size, "Expected one UInt32 (four bytes) per vertex index")
        let vertexCountPerFace = faces.indexCountPerPrimitive
        let vertexIndicesPointer = faces.buffer.contents()
        var vertexIndices = [UInt32]()
        vertexIndices.reserveCapacity(vertexCountPerFace)
        for vertexOffset in 0..<vertexCountPerFace {
            let vertexIndexPointer = vertexIndicesPointer.advanced(by: (faceIndex * vertexCountPerFace + vertexOffset) * MemoryLayout<UInt32>.size)
            vertexIndices.append(vertexIndexPointer.assumingMemoryBound(to: UInt32.self).pointee)
        }
        return vertexIndices
    }
    
    func verticesOf(faceWithIndex index: Int) -> [(Float, Float, Float)] {
        let vertexIndices = vertexIndicesOf(faceWithIndex: index)
        let vertices = vertexIndices.map { vertex(at: $0) }
        return vertices
    }
    
    func centerOf(faceWithIndex index: Int) -> (Float, Float, Float) {
        let vertices = verticesOf(faceWithIndex: index)
        let sum = vertices.reduce((0, 0, 0)) { ($0.0 + $1.0, $0.1 + $1.1, $0.2 + $1.2) }
        let geometricCenter = (sum.0 / 3, sum.1 / 3, sum.2 / 3)
        return geometricCenter
    }
    
    func toMDLMesh(device: MTLDevice) -> MDLMesh {
        let allocator = MTKMeshBufferAllocator(device: device);

        let data = Data.init(bytes: vertices.buffer.contents(), count: vertices.stride * vertices.count);
        let vertexBuffer = allocator.newBuffer(with: data, type: .vertex);

        let indexData = Data.init(bytes: faces.buffer.contents(), count: faces.bytesPerIndex * faces.count * faces.indexCountPerPrimitive);
        let indexBuffer = allocator.newBuffer(with: indexData, type: .index);

        let submesh = MDLSubmesh(indexBuffer: indexBuffer,
                                 indexCount: faces.count * faces.indexCountPerPrimitive,
                                 indexType: .uInt32,
                                 geometryType: .triangles,
                                 material: nil);

        let vertexDescriptor = MDLVertexDescriptor();
        vertexDescriptor.attributes[0] = MDLVertexAttribute(name: MDLVertexAttributePosition,
                                                            format: .float3,
                                                            offset: 0,
                                                            bufferIndex: 0);
        vertexDescriptor.layouts[0] = MDLVertexBufferLayout(stride: vertices.stride);

        return MDLMesh(vertexBuffer: vertexBuffer,
                       vertexCount: vertices.count,
                       descriptor: vertexDescriptor,
                       submeshes: [submesh]);
    }
}

func translateTransform(_ x: Float, _ y: Float, _ z: Float) -> float4x4 {
    var tf = float4x4(diagonal: SIMD4<Float>(repeating: 1))
    tf.columns.3 = SIMD4<Float>(x: x, y: y, z: z, w: 1)
    return tf
}

extension ARPlaneAnchor {
    
    // returns all 4 world coordinates of the given plane
    // (topLeft, topRight, bottomLeft, bottomRight)
    func worldPoints() -> (SCNVector3, SCNVector3, SCNVector3, SCNVector3) {
        
        // Get world's updated center
        let worldTransform = transform * translateTransform(center.x, 0, center.z)
        
        let width = extent.x
        let height = extent.z

        let topLeft = worldTransform * translateTransform(-width / 2.0, 0, -height / 2.0)
        let topRight = worldTransform * translateTransform(width / 2.0, 0, -height / 2.0)
        let bottomLeft = worldTransform * translateTransform(-width / 2.0, 0, height / 2.0)
        let bottomRight = worldTransform * translateTransform(width / 2.0, 0, height / 2.0)

       
        let pointTopLeft = SCNVector3(
            x: topLeft.columns.3.x,
            y: topLeft.columns.3.y,
            z: topLeft.columns.3.z
        )
        
        let pointTopRight = SCNVector3(
            x: topRight.columns.3.x,
            y: topRight.columns.3.y,
            z: topRight.columns.3.z
        )
        
        let pointBottomLeft = SCNVector3(
            x: bottomLeft.columns.3.x,
            y: bottomLeft.columns.3.y,
            z: bottomLeft.columns.3.z
        )
        
        let pointBottomRight = SCNVector3(
            x: bottomRight.columns.3.x,
            y: bottomRight.columns.3.y,
            z: bottomRight.columns.3.z
        )
        
        return (
            pointTopLeft,
            pointTopRight,
            pointBottomLeft,
            pointBottomRight
        )
    }
}

