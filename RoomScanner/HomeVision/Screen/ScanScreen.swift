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
            
            ControlBar()
        }
        .edgesIgnoringSafeArea(.all)
    }
}

struct ControlBar: View {
    var body: some View {
        HStack{
            Button(action: {print("button pressed")}){Text("Export")}
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
    }
}
