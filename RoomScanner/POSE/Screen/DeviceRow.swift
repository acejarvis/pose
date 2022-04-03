//
//  DeviceRow.swift
//  POSE
//
//  Created by Ethan on 2022/4/2.
//

import SwiftUI

struct DeviceRow: View {

   var devices = DevicesData

   var body: some View {
      VStack(alignment: .leading) {
         Text("Supported Devices")
            .font(.system(size: 30))
            .fontWeight(.heavy)
            .padding(.leading, 30)

         ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 20) {
               ForEach(devices) { item in
                   DeviceView(item: item)
               }
            }
            .padding(20)
            .padding(.leading, 10)
         }
      }
   }
}

struct DeviceView: View {
   var item = Device(title: "Camera", image: "Certificate1", width: 340, height: 220)

   var body: some View {
      return VStack {
         HStack {
            VStack(alignment: .leading) {
               Text(item.title)
                  .font(.headline)
                  .fontWeight(.bold)
                  .foregroundColor(Color.black)
                  .padding(.top)
            }
            Spacer()

//            Image("Logo")
//               .resizable()
//               .frame(width: 38, height: 30)
         }
         .padding(.horizontal)
         Spacer()

         Image(item.image)
              .frame(minWidth: 0, maxWidth: .infinity, minHeight: 0, maxHeight: .infinity)
//            .offset(y: 50)
      }
      .frame(width: CGFloat(item.width), height: CGFloat(item.height))
      .background(Color.white)
      .cornerRadius(10)
      .shadow(radius: 10)
      .opacity(0.9)
   }
}

#if DEBUG
struct CertificateRow_Previews: PreviewProvider {
   static var previews: some View {
       DeviceRow()
   }
}
#endif

struct Device: Identifiable {
   var id = UUID()
   var title: String
   var image: String
   var width: Int
   var height: Int
}

let DevicesData = [
    Device(title: "Camera", image: "device1", width: 230, height: 150),
    Device(title: "Light", image: "device2", width: 230, height: 150),
    Device(title: "Speaker", image: "device3", width: 230, height: 150),
    Device(title: "TV", image: "device4", width: 230, height: 150)
]


