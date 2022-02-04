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
    
    var body: some View {
        ZStack(alignment: .bottom){
            ARViewContainer(viewModel: viewModel)
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
    
    func makeUIView(context: Context) -> MyARView {
        let arView = MyARView(frame: .zero, viewModel: viewModel)
        
        return arView
    }
    
    func updateUIView(_ uiView: MyARView, context: Context) {
        if(viewModel.ClearAnchorObjects){
            uiView.clearAnchorObjects()
        }
    }
}



struct ScanScreen_Previews: PreviewProvider {
    static var previews: some View {
        ScanScreen()
            .previewInterfaceOrientation(.landscapeRight)
            .previewDevice("iPad Pro (11-inch) (3rd generation)")
    }
}
