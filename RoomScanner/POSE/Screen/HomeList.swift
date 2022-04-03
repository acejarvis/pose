//
//  HomeList.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct HomeList: View {
   @State var showContent = false

   var body: some View {
       NavigationView{
          ScrollView {
             VStack {
                HStack {
                   VStack(alignment: .leading) {
                      Text("P.O.S.E")
                         .font(.largeTitle)
                         .fontWeight(.heavy)

                      Text("Welcome to Pointing Oriented SmartHome Environment\nYour Exclusive Smart Home Experience")
                         .foregroundColor(.gray)
                   }
                   Spacer()
                }
                .padding(.leading, 60.0)
                 
                 ScrollView(.horizontal, showsIndicators: false) {
                         HStack(spacing: 30.0) {
                             NavigationLink(destination: ScanScreen(), label: {Card1()})
                             
                             NavigationLink(destination: ObjView(), label: {Card2()})
                             
                             NavigationLink(destination: WhyPOSEView(), label: {Card3()})
                             
                             NavigationLink(destination: DeveloperTeam(), label: {Card4()})
                         }
                         .padding(.leading, 30)
                         .padding(.top, 30)
                         .padding(.bottom, 70)
                         Spacer()
                 }
                 
                 DeviceRow()
             }

          }
               
       }
       .navigationViewStyle(StackNavigationViewStyle())
       .navigationBarTitle(Text("Home"))
       .navigationBarHidden(true)
//       .edgesIgnoringSafeArea([.top, .bottom])
       .ignoresSafeArea()
    }

}

struct Card1: View{
    var AppFunctions = AppFunctionData
    
    var body: some View{
        HStack {
            GeometryReader { geometry in
               CourseView(title: AppFunctions[0].title,
                          image: AppFunctions[0].image,
                          color: AppFunctions[0].color,
                          shadowColor: AppFunctions[0].shadowColor)
                  .rotation3DEffect(Angle(degrees:
                     Double(geometry.frame(in: .global).minX - 30) / -40), axis: (x: 0, y: 10.0, z: 0))

            }
            .frame(width: 246, height: 360)
        }
    }
}

struct Card2: View{
    var AppFunctions = AppFunctionData
    
    var body: some View{
        HStack {
            GeometryReader { geometry in
               CourseView(title: AppFunctions[1].title,
                          image: AppFunctions[1].image,
                          color: AppFunctions[1].color,
                          shadowColor: AppFunctions[1].shadowColor)
                  .rotation3DEffect(Angle(degrees:
                     Double(geometry.frame(in: .global).minX - 30) / -40), axis: (x: 0, y: 10.0, z: 0))

            }
            .frame(width: 246, height: 360)
        }
    }
}

struct Card3: View{
    var AppFunctions = AppFunctionData
    
    var body: some View{
        HStack {
            GeometryReader { geometry in
               CourseView(title: AppFunctions[2].title,
                          image: AppFunctions[2].image,
                          color: AppFunctions[2].color,
                          shadowColor: AppFunctions[2].shadowColor)
                  .rotation3DEffect(Angle(degrees:
                     Double(geometry.frame(in: .global).minX - 30) / -40), axis: (x: 0, y: 10.0, z: 0))

            }
            .frame(width: 246, height: 360)
        }
    }
}

struct Card4: View{
    var AppFunctions = AppFunctionData
    
    var body: some View{
        HStack {
            GeometryReader { geometry in
               CourseView(title: AppFunctions[3].title,
                          image: AppFunctions[3].image,
                          color: AppFunctions[3].color,
                          shadowColor: AppFunctions[3].shadowColor)
                  .rotation3DEffect(Angle(degrees:
                     Double(geometry.frame(in: .global).minX - 30) / -40), axis: (x: 0, y: 10.0, z: 0))

            }
            .frame(width: 246, height: 360)
        }
    }
}

#if DEBUG
struct HomeList_Previews: PreviewProvider {
   static var previews: some View {
       HomeList()
           .previewInterfaceOrientation(.landscapeLeft)
           .previewDevice("iPad Pro (11-inch) (3rd generation)")
   }
}
#endif

struct CourseView: View {

   var title = "Set Up My Devices"
   var image = "Illustration1"
   var color = Color("background3")
   var shadowColor = Color("backgroundShadow3")

   var body: some View {
      return VStack(alignment: .leading) {
         Text(title)
            .font(.title)
            .fontWeight(.bold)
            .foregroundColor(.white)
            .padding(30)
            .lineLimit(4)

         Spacer()

         Image(image)
            .resizable()
            .renderingMode(.original)
            .aspectRatio(contentMode: .fit)
            .frame(width: 246, height: 150)
            .padding(.bottom, 30)
      }
      .background(color)
      .cornerRadius(30)
      .frame(width: 246, height: 360)
//      .shadow(color: shadowColor, radius: 10, x: 0, y: 20)
   }
}

struct AppFunction: Identifiable {
   var id = UUID()
   var title: String
   var image: String
   var color: Color
   var shadowColor: Color
}

let AppFunctionData = [
    AppFunction(title: "Set Up My Devices",
          image: "Illustration1",
          color: Color("background11"),
          shadowColor: Color("background11")),
    AppFunction(title: "My Room",
          image: "Illustration2",
          color: Color(("background12")),
          shadowColor: Color("background12")),
    AppFunction(title: "Why P.O.S.E",
          image: "Illustration3",
          color: Color("background13"),
          shadowColor: Color("background13")),
    AppFunction(title: "Developer Team",
          image: "Illustration4",
          color: Color("background14"),
          shadowColor: Color("background14")),
]
