//
//  HomeScreen.swift
//  POSE
//
//  Created by Ethan on 2022/3/30.
//

import SwiftUI

struct HomeScreen: View {
    var body: some View {
        NavigationView{
            ZStack {
                Color(#colorLiteral(red: 0.937254902, green: 0.937254902, blue:0.937254902, alpha: 1))
                    .ignoresSafeArea()
                
                VStack {
                    TagLineView()
                    
                    Spacer()
                    
                    NavigationLink(destination: ScanScreen(), label: {SetUpView()})
                    
                    Spacer()
                }
            }
        }
        .navigationViewStyle(StackNavigationViewStyle())
    }
}

struct TagLineView: View {
    var body: some View {
        HStack{
            Text("WELCOME\nTO\nP.O.S.E")
                .font(.largeTitle)
                .fontWeight(.bold)
                .foregroundColor(.black)
                .multilineTextAlignment(.center)
        }
        .padding()
    }
}

struct SetUpView: View {
    var body: some View {
        HStack{
            Text("Set Up\nYour Device")
                .foregroundColor(.white)
                .font(.title)
                .padding(30)
                .multilineTextAlignment(.center)
        }
        .background(.black)
        .cornerRadius(20)
        .padding()
    }
}

struct HomeScreen_Previews: PreviewProvider {
    static var previews: some View {
        HomeScreen()
            .previewDevice("iPad Pro (11-inch) (3rd generation)")
    }
}

