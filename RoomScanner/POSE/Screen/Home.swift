//
//  Home.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct Home: View {

   @State var show = false
   @State var showProfile = false

   var body: some View {
      ZStack(alignment: .top) {
         HomeList()
            .blur(radius: show ? 20 : 0)
            .scaleEffect(showProfile ? 0.95 : 1)
      }
      .edgesIgnoringSafeArea(.all)
   }
}

#if DEBUG
struct Home_Previews: PreviewProvider {
   static var previews: some View {
       Home()
           .previewInterfaceOrientation(.portrait)
           .previewDevice("iPhone 13 Pro")
   }
}
#endif
