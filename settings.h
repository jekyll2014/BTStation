#pragma once
// ������ ��������, ����� ������� � ����
#define FW_VERSION        111

#define UART_SPEED        38400

#define BT_COMMAND_ENABLE 2 // ������ ������ �������� ������� � ������ Bluetooth

#define RED_LED_PIN       A1 // ��������� ������ (�������)
#define GREEN_LED_PIN     4 // ��������� ������������� (�������)
#define BUZZER_PIN        3 // �������

#define FLASH_SS_PIN      8 // SPI SELECT pin
#define FLASH_ENABLE_PIN  7 // SPI enable pin

#define RFID_RST_PIN      9 // RFID ������ reset
#define RFID_SS_PIN       10 // RFID ������ chip_select
//#define RFID_MOSI_PIN     11 // RFID ������
//#define RFID_MISO_PIN     12 // RFID ������
//#define RFID_SCK_PIN      13 // RFID ������

#define BATTERY_PIN       A0 // ����� ���������� �������
#define BATTERY_ALARM_COUNT 100 // ������� ������ ������� ���� ����� �������� � ������������ �������
#define RTC_ENABLE_PIN    5 // ������� �����
#define RTC_ALARM_DELAY     10000 // �������� ����� ��������� ���� �����

// ����-��� ������ ������� � ������� ������
#define RECEIVE_TIMEOUT   1000

// ������������� ������ ����
#define RFID_READ_PERIOD  1000

// ������ ������ ��������� ������
#define LAST_TEAMS_LENGTH 10

// ������ ������ ��������� ������
#define LAST_ERRORS_LENGTH 10
#define MAX_PAKET_LENGTH	256
