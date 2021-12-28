#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>              
#define SERVICE_UUID             "705cc7b4-ad8d-40ba-9583-fcb7819ed284"
#define CHARACTERISTIC1_UUID     "705cc7b4-ad8d-40ba-9583-fcb7819ed284"
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
        pCharacteristic->notify();
      }
    }
};
void setupBLE()
{
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
  pCharacteristic->setValue("PAGE 1");
  pService->start();
  BLEAdvertising *pAdvertising = pServer->getAdvertising();
  pAdvertising->start();
}

// Pins for Reading

int pin_page_1_2 = 2;
int pin_page_3_4 = 3;
int pin_page_5_6 = 4;
int pin_page_7_8 = 5;
int pin_page_9_10 = 6;

void setup() {
  Serial.begin(115200);
  setupBLE();

  // Setup read pins 
  
}

void loop() {
   delay(3000);
}
