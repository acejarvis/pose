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
    var body: some View {
        ZStack(alignment: .bottom){
            ARViewContainer()
                .edgesIgnoringSafeArea(.all)
            
            bottomControlBar()
            
        }
        .edgesIgnoringSafeArea(.all)
    }
}

struct bottomControlBar: View {
    var body: some View {
        HStack(alignment: .bottom){
            Spacer()
            
            Button(action: {print("Reset button pressed")}){
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
}

struct ARViewContainer: UIViewRepresentable {
    func makeUIView(context: Context) -> ARView {
        let CustomArView = ViewController(frame: .zero)
        
        return CustomArView
    }
    
    func updateUIView(_ uiView: ARView, context: Context) {}
}



struct ScanScreen_Previews: PreviewProvider {
    static var previews: some View {
        ScanScreen()
            .previewInterfaceOrientation(.landscapeRight)
            .previewDevice("iPad Pro (11-inch) (3rd generation)")
    }
}
