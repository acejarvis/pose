//
//  ObjView.swift
//  POSE
//
//  Created by Ethan on 2022/3/30.
//

import SwiftUI
import Model3DView

struct ObjView: View{
    @State var camera = PerspectiveCamera()
    @StateObject var viewModel = MyViewModel()
     
    var body: some View{
        ZStack(alignment: .bottom){
            VStack(){
                Model3DView(file: viewModel.ObjFilePath)
                    .transform(scale: 1.2)
                    .cameraControls(OrbitControls(
                        camera: $camera,
                        sensitivity: 0.3
                    ))
            }
            .background(.black)
            .ignoresSafeArea()
        }

    }
}
