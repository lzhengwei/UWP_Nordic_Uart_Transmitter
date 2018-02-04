# UWP Nordic Uart Transmitter

This is a Windows10 UWP C# project.

Goal of this project is connect to Nordic IC nrf51822(MDBT40) through Ble(Bluetooth Low Energy).

This program has basic uart function include **list and connect to ble device**、**list Ble device's UUID**、**send and receive data***

Divide into two project

| Project  | function |
| ---------- | ----------- |
| BLE_GATT_list  | List all Ble divice GATT UUID |
| BLE_uart_connector | As a connector to Ble divice, Use Nordic Uart UUID to sned/receive data form device |

# Artitecture 

![artitecture](https://github.com/lzhengwei/UWP_Nordic_Uart_Transmitter/blob/master/Structure.jpg)
