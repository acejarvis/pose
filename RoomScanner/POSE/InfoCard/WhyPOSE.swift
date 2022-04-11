//
//  whyPOSE.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct WhyPOSEView: View{
    let documentURL = Bundle.main.url(forResource: "Project Proposal", withExtension: "pdf")!
    var body: some View{
        VStack(alignment: .leading){
            PDFKitView(url: documentURL)
                .ignoresSafeArea()
        }
    }
}

#if DEBUG
struct WhyPOSEView_Previews: PreviewProvider {
   static var previews: some View {
       WhyPOSEView()
           .previewInterfaceOrientation(.landscapeLeft)
           .previewDevice("iPad Pro (11-inch) (3rd generation)")
   }
}
#endif
