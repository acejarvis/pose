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
                Image("home")
                    .resizable()
                    .ignoresSafeArea()
                
                VStack {
                    TagLineView()
                    
                    Spacer()

                    VStack{
                        NavigationLink(destination: ScanScreen(), label: {SetUpView()})
                        
                        NavigationLink(destination: ObjView(), label: {ObjViwerView()})
                    }
                    
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
                .opacity(0.8)
        }
        .padding()
    }
}

struct SetUpView: View {
    var body: some View {
        HStack{
            Text("Set Up\nMy Device")
                .foregroundColor(.white)
                .font(.title)
                .padding(30)
                .multilineTextAlignment(.center)
                .frame(width: 250, height: 150)
        }
        .background(.black)
        .opacity(0.85)
        .cornerRadius(20)
        .padding()
    }
}

struct ObjViwerView: View {
    var body: some View {
        HStack{
            Text("My Room")
                .foregroundColor(.white)
                .font(.title)
                .padding(30)
                .multilineTextAlignment(.center)
                .frame(width: 250, height: 150)
        }
        .background(.black)
        .opacity(0.85)
        .cornerRadius(20)
        .padding()
    }
}

struct HomeScreen_Previews: PreviewProvider {
    static var previews: some View {
        HomeScreen()
            .previewDevice("iPad Pro (11-inch) (3rd generation)")
            .previewInterfaceOrientation(.landscapeLeft)
    }
}

