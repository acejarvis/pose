//
//  DeveloperTeam.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct DeveloperTeam: View{
    let documentURL = Bundle.main.url(forResource: "team", withExtension: "pdf")!
    var body: some View{
        VStack(alignment: .leading){
            PDFKitView(url: documentURL)
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
