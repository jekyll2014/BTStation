#pragma once
// версия прошивки, номер пишется в чипы
#define FW_VERSION        110

#define UART_SPEED        38400

#define BT_COMMAND_ENABLE 2 // светодиод ошибки (красный)

#define RED_LED_PIN       A1 // светодиод ошибки (красный)
#define GREEN_LED_PIN     4 // светодиод синий
#define BUZZER_PIN        3 // пищалка

#define FLASH_SS_PIN      8 // SPI SELECT pin
#define FLASH_ENABLE_PIN  7 // SPI enable pin

#define RFID_RST_PIN      9 // рфид модуль reset
#define RFID_SS_PIN       10 // рфид модуль chip_select
//#define RFID_MOSI_PIN     11 // рфид модуль
//#define RFID_MISO_PIN     12 // рфид модуль
//#define RFID_SCK_PIN      13 // рфид модуль

#define BATTERY_PIN       A0 // замер напряжения батареи

#define RTC_ENABLE_PIN    5 // питание часов

// тайм-аут приема команды с момента начала
#define RECEIVE_TIMEOUT   1000

// периодичность поиска чипа
#define RFID_READ_PERIOD  1000

// размер буфера последних команд
#define LAST_TEAMS_LENGTH 10

// размер буфера последних команд
#define LAST_ERRORS_LENGTH 10
