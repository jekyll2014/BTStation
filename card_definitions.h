#pragma once
// �������� � ����. 0-7 ���������, 8-... ��� �������
#define PAGE_UID1         0
#define PAGE_UID2         1
#define PAGE_CHIP_SYS1    2
#define PAGE_CHIP_SYS2    3 // system[2] + ���_����[1] + system[1]
#define PAGE_CHIP_NUM     4 // �����_����[2] + reserved[1] + ������_��������[1]
#define PAGE_INIT_TIME    5 // ����� �������������[4]
#define PAGE_TEAM_MASK    6 // ����� �������[2] + reserved[2]
#define PAGE_RESERVED     7 // reserved for future use[4]
#define PAGE_DATA_START   8 // 1st data page: ����� ��[1] + ����� ��������� ��[3]

#define PAGE_DYNAMIC_LOCK 0 // dynamic lock bytes, RFUI
#define PAGE_CFG0         1 // CFG 0
#define PAGE_CFG1         2 // CFG 1
#define PAGE_PWD          3 // PWD
#define PAGE_PACK         4 // PACK, RFUI1

#define NTAG213_ID				0x12
#define NTAG213_MAX_PAGE	40

#define NTAG215_ID				0x3e
#define NTAG215_MAX_PAGE	130

#define NTAG216_ID				0x6d
#define NTAG216_MAX_PAGE	226
