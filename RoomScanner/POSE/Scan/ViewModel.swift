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
