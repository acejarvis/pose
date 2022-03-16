//
//  ViewModel.swift
//  HomeVision
//
//  Created by Ethan on 2022/2/3.
//

import Foundation

public class MyViewModel: ObservableObject {
    @Published public var ClearAnchorObjects: Bool = false
    @Published public var DonePressed: Bool = false
    @Published public var cameraError = false
}

struct PlacingObjectModel {
    enum ObjType: String, CaseIterable, Identifiable {
        case camera
        case light
        case speaker
        
        var id: String {self.rawValue}
    }
    var objType: ObjType = .camera
}
