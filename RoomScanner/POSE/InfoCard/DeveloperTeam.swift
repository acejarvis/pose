//
//  DeveloperTeam.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct DeveloperTeam: View{
     
    var body: some View{
        ZStack(alignment: .bottom){
            VStack(){
                Text("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
            }
            .ignoresSafeArea()
        }

    }
}

#if DEBUG
struct DeveloperTeam_Previews: PreviewProvider {
   static var previews: some View {
       DeveloperTeam()
           .previewInterfaceOrientation(.landscapeLeft)
           .previewDevice("iPad Pro (11-inch) (3rd generation)")
   }
}
#endif
