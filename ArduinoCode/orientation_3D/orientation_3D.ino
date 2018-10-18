#include <Pozyx.h>
#include <Pozyx_definitions.h>
#include <Wire.h>

boolean remote = false;               // boolean to indicate if we want to read sensor data from the attached pozyx shield (value 0) or from a remote pozyx device (value 1)
uint16_t remote_id = 0x690f;          // the network id of the other pozyx device: fill in the network id of the other device
uint32_t last_millis = 0;                 // used to compute the measurement interval in milliseconds 
uint32_t start_millis = 0;
bool establish_COM = true;
void setup()
{  
  Serial.begin(115200);
    
  if(Pozyx.begin(false, MODE_INTERRUPT, POZYX_INT_MASK_IMU) == POZYX_FAILURE){
    Serial.println("ERROR: Unable to connect to POZYX shield");
    Serial.println("Reset required");
    delay(100);
    abort();
  }
  if(!remote)
    remote_id = NULL;
  last_millis = millis();
  delay(10);  
  start_millis = millis();
}

void loop(){
if(establish_COM) {
    if (millis() - start_millis < 5000 ) {
      Serial.println("INS");
    } else {
      establish_COM = false;
    }
  } else {
    sensor_raw_t sensor_raw;
    uint8_t calibration_status = 0;
    int dt;
    int status;
    if(remote){
       status = Pozyx.getRawSensorData(&sensor_raw, remote_id);
       status &= Pozyx.getCalibrationStatus(&calibration_status, remote_id);
      if(status != POZYX_SUCCESS){
        return;
      }
    }else{
      if (Pozyx.waitForFlag(POZYX_INT_STATUS_IMU, 10) == POZYX_SUCCESS){
        Pozyx.getRawSensorData(&sensor_raw);
        Pozyx.getCalibrationStatus(&calibration_status);
      }else{
        uint8_t interrupt_status = 0;
        Pozyx.getInterruptStatus(&interrupt_status);
        return;
      }
    }
  
    dt = millis() - last_millis;
    last_millis += dt;    
    // print time difference between last measurement in ms, sensor data, and calibration data
    printRawSensorData(sensor_raw, dt);
    // will be zeros for remote devices as unavailable remotely.
    // printCalibrationStatus(calibration_status);
    }
}

void printRawSensorData(sensor_raw_t sensor_raw, int dt) {
  Serial.print("timer:");
  Serial.println(dt);
  
  Serial.print("AC");
  Serial.print(sensor_raw.linear_acceleration[0]);
  Serial.print(":");
  Serial.print(sensor_raw.linear_acceleration[1]);
  Serial.print(":");
  Serial.println(sensor_raw.linear_acceleration[2]);
  
  Serial.print("GY");
  Serial.print(sensor_raw.angular_vel[0]);
  Serial.print(":");
  Serial.print(sensor_raw.angular_vel[1]);
  Serial.print(":");
  Serial.println(sensor_raw.angular_vel[2]);
}

void printCalibrationStatus(uint8_t calibration_status){
  Serial.print(calibration_status & 0x03);
  Serial.print(",");
  Serial.print((calibration_status & 0x0C) >> 2);
  Serial.print(",");
  Serial.print((calibration_status & 0x30) >> 4);
  Serial.print(",");
  Serial.print((calibration_status & 0xC0) >> 6);  
}
