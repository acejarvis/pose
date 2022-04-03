//
//  ViewModel.swift
//  POSE
//
//  Created by Ethan on 2022/3/30.
//

import Foundation

public struct ErrorInfo: Identifiable {
    public var id: Int
    public var title: String
    public var description: String
}

public class MyViewModel: ObservableObject {
    @Published public var ClearAnchorObjects: Bool = false
    @Published public var DonePressed: Bool = false
    @Published public var PopError: ErrorInfo?
    @Published public var ObjFilePath: URL
    
    init(){
        let documentsPath = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask).first!
        ObjFilePath = documentsPath.appendingPathComponent("room.obj")
    }
}

struct PlacingObjectModel {
    enum ObjType: String, CaseIterable, Identifiable {
        case camera
        case light
        case speaker
        case TV
        
        var id: String {self.rawValue}
    }
    var objType: ObjType = .camera
}
