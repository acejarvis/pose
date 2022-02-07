//
//  ScanScreen.swift
//  HomeVision
//
//  Created by Ethan on 2022/1/12.
//

import SwiftUI
import RealityKit
import ARKit

struct ScanScreen: View {
    @StateObject var viewModel = MyViewModel()
    @State private var objModel = PlacingObjectModel()
    
    var body: some View {
        ZStack(alignment: .bottom){
            ARViewContainer(viewModel: viewModel, objModel: $objModel)
                .edgesIgnoringSafeArea(.all)
            
            HStack(alignment: .bottom){
                Spacer()
                
                Button(
                    action: {
                        viewModel.ClearAnchorObjects = true
                    }){
                    Text("Reset")
                        .frame(maxWidth: 60)
                        .font(.system(size: 15))
                        .padding()
                        .foregroundColor(.white)
                    
                }
                .background(Color.blue)
                .cornerRadius(15)
                
                Spacer()
                
                Menu(content: {
                    Picker("", selection: $objModel.objType) {
                        Text(PlacingObjectModel.ObjType.camera.rawValue).tag(PlacingObjectModel.ObjType.camera)
                            .foregroundColor(.white)
                        Text(PlacingObjectModel.ObjType.light.rawValue).tag(PlacingObjectModel.ObjType.light)
                            .foregroundColor(.white)
                        Text(PlacingObjectModel.ObjType.speaker.rawValue).tag(PlacingObjectModel.ObjType.speaker)
                            .foregroundColor(.white)
                    }
                    .padding()
                },
                 label: { Label ("", systemImage: "plus.circle").font(.system(size: 40)) })
                
                Spacer()
                
                Button(action: {print("Done button pressed")}){
                    Text("Done")
                        .frame(maxWidth: 60)
                        .font(.system(size: 15))
                        .padding()
                        .foregroundColor(.white)
                }
                .background(Color.blue)
                .cornerRadius(15)
                
                Spacer()
            }
            .frame(maxWidth: .infinity)
            .padding()
            .padding(.bottom)
            .background(Color.black.opacity(0.25))
            
        }
        .edgesIgnoringSafeArea(.all)
    }
}

struct ARViewContainer: UIViewRepresentable {
    @ObservedObject var viewModel: MyViewModel
    @Binding var objModel:PlacingObjectModel
    
    func makeUIView(context: Context) -> MyARView {
        let arView = MyARView(frame: .zero, viewModel: viewModel)
        
        return arView
    }
    
    func updateUIView(_ uiView: MyARView, context: Context) {
        if(viewModel.ClearAnchorObjects){
            uiView.clearAnchorObjects()
        }
        
        uiView.objModel = objModel
    }
}



struct ScanScreen_Previews: PreviewProvider {
    static var previews: some View {
        ScanScreen()
            .previewInterfaceOrientation(.landscapeRight)
            .previewDevice("iPad Pro (11-inch) (3rd generation)")
    }
}
