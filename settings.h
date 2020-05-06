#pragma once
// ������ ��������, ����� ������� � ����
#define FW_VERSION        109

#define UART_SPEED        38400

#define BT_COMMAND_ENABLE 2 // ��������� ������ (�������)

#define RED_LED_PIN       A1 // ��������� ������ (�������)
#define GREEN_LED_PIN     4 // ��������� �����
#define BUZZER_PIN        3 // �������

#define FLASH_SS_PIN      8 // SPI SELECT pin
#define FLASH_ENABLE_PIN  7 // SPI enable pin

#define RFID_RST_PIN      9 // ���� ������ reset
#define RFID_SS_PIN       10 // ���� ������ chip_select
//#define RFID_MOSI_PIN     11 // ���� ������
//#define RFID_MISO_PIN     12 // ���� ������
//#define RFID_SCK_PIN      13 // ���� ������

#define BATTERY_PIN       A0 // ����� ���������� �������

#define RTC_ENABLE_PIN    5 // ������� �����

// ����-��� ������ ������� � ������� ������
#define receiveTimeOut    1000

// ������������� ������ ����
#define rfidReadPeriod    1000

// ������ ������ ��������� ������
#define lastTeamsLength   10
