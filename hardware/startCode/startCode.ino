#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>              
#define SERVICE_UUID             "705cc7b4-ad8d-40ba-9583-fcb7819ed284"
#define CHARACTERISTIC1_UUID     "705cc7b4-ad8d-40ba-9583-fcb7819ed284"

#define pin_page_1_2  10
#define pin_page_3_4  11
#define pin_page_5_6  3
#define pin_page_7_8  2



bool startChecking = false;
bool deviceConnected = false;

BLEServer *pServer;
BLEService *pService;
BLECharacteristic* pCharacteristic;
class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      deviceConnected = true;
    };

    void onDisconnect(BLEServer* pServer) {
      deviceConnected = false;
    }
};
class MyCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
      std::string value = pCharacteristic->getValue();

      if (value.length() > 0) {
        Serial.println("*********");
        Serial.print("New value: ");
        for (int i = 0; i < value.length(); i++){
          Serial.print(value[i]);
        }
        Serial.println();
        Serial.println("*********");
        startChecking = true;
        pCharacteristic->notify();
      }
    }
};

void setupBLE()
{
  
  
  // Setup BLE device
  Serial.println("Attemping Bluetooth init");
  BLEDevice::init("Fred the Forgetful Frog");   //Create BLE device 
  pServer = BLEDevice::createServer();   //Create BLE server 
  pServer->setCallbacks(new MyServerCallbacks());   //Set the callback function of the server 
  pService = pServer->createService(SERVICE_UUID); //Create BLE service 
  pCharacteristic = pService->createCharacteristic(
                                                 CHARACTERISTIC1_UUID,
                                                 BLECharacteristic::PROPERTY_READ|
                                                 BLECharacteristic::PROPERTY_NOTIFY|
                                                 BLECharacteristic::PROPERTY_WRITE);   //Create the characteristic value of the service 
  pCharacteristic->setCallbacks(new MyCallbacks());    //Set the callback function of the chracteristic value 
  pCharacteristic->addDescriptor(new BLE2902());
  pCharacteristic->setValue("-1");
  pService->start();
  BLEAdvertising *pAdvertising = pServer->getAdvertising();
  pAdvertising->start();
  Serial.println("Bluetooth initalized\n");
}



void setup() {
  Serial.begin(115200);
  setupBLE();
  // Setup read pins
 
  
}


void loop() {
   delay(3000);
   if (!startChecking)
   {
      int a = digitalRead(pin_page_1_2);
      int b = digitalRead(pin_page_3_4);
      int c = digitalRead(pin_page_5_6);
     Serial.print(a); Serial.print(", ");
     Serial.print(b); Serial.print(", ");
     Serial.println(c);
    
     if (touchRead(14) != 0)
     {
      pCharacteristic->setValue("1");
      Serial.println("Page 1-2 detected\n");
     }
     else if (touchRead(0) != 0)
     {
      pCharacteristic->setValue("3");
      Serial.println("Page 3-4 detected\n");
     }
     else if (touchRead(12) != 0)
     {
      pCharacteristic->setValue("5");
      Serial.println("Page 5-6 detected\n");
     }
     else if (touchRead(4) != 0)
     {
      pCharacteristic->setValue("7");
      Serial.println("Page 7-8 detected\n");
     }
     else
     {
     // pCharacteristic->setValue("-1");
      Serial.println("No Open pages detected\n");
     }
   }
   
  
  
}
