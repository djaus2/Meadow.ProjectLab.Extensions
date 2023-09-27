# Meadow.ProjectLab.Extensions

Extended demo projects for Meadow ProjectLab.

![The board](./theboard.png)  
**_The WILDERNESS LABS Project Lab V3 board_**

## Links
- The target board:  [The Wilderness Labs project V3](https://store.wildernesslabs.co/collections/frontpage/products/project-lab-board)
- [Meadow ProjectLab_Demo](https://github.com/WildernessLabs/Meadow.ProjectLab/tree/main/Source/)
- [WildernessLabs/Meadow.ProjectLab](https://github.com/WildernessLabs/Meadow.ProjectLab)
- [Meadow ProjectLab Samples](https://github.com/WildernessLabs/Meadow.ProjectLab.Samples)
- [Meadow.Core.Samples](https://github.com/WildernessLabs/Meadow.Core.Samples)
- [Meadow.Foundation.Grove](https://github.com/WildernessLabs/Meadow.Foundation.Grove)

> Nb: Currently .netstandard2.1 projects

## WiFi Demo

This project is based upon the [Meadow ProjectLab_Demo](https://github.com/WildernessLabs/Meadow.ProjectLab/tree/main/Source/). It needs the project **Meadow.ProjectLab** located with that project. _(You need to clone that GitHub project and then add the Meadow.ProjectLab project as a project reference to this project.)_ 

The WiFi functionality is based upon the [Meadow.Core.Samples WiFi_Basics](https://github.com/WildernessLabs/Meadow.Core.Samples/tree/main/Source/Network/WiFi_Basics/CS) project. 
> This project demonstrates using the display in a reusable manner, as a menu. The user selects a network from the locally found WiFi access poinst and then selects from a given list of passwords.

### Further Links
- [UeberDaniel/Meadow-Ws2812Display-Driver](https://github.com/UeberDaniel/Meadow-Ws2812Display-Driver)
- Alternaive approach: [MicroLayout Library](http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/Libraries_and_Frameworks/MicroLayout/)

## SerialPort_Echo

Demonstrates loopback serial transmission.  
- Uses COM1. 
- Jumper COM1 Tx to Rx in mikroBUS socket 1 _(Not socket2)_
- Adapdted from **[Meadow.Core.Samples]/IO/SerialPort_Echo project**





