object fmFilter: TfmFilter
  Left = 529
  Top = 146
  BorderStyle = bsDialog
  Caption = #1060#1080#1083#1100#1090#1088
  ClientHeight = 448
  ClientWidth = 297
  Color = clBtnFace
  DefaultMonitor = dmMainForm
  Font.Charset = RUSSIAN_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 8
    Top = 160
    Width = 64
    Height = 13
    Caption = #1052#1072#1089#1082#1072' '#1080#1084#1077#1085#1080
  end
  object Label2: TLabel
    Left = 8
    Top = 112
    Width = 63
    Height = 13
    Caption = #1042' '#1078#1080#1074#1099#1093' '#1076#1086':'
  end
  object Label3: TLabel
    Left = 8
    Top = 208
    Width = 124
    Height = 13
    Caption = #1052#1072#1089#1082#1072' '#1084#1077#1089#1090#1086#1078#1080#1090#1077#1083#1100#1089#1090#1074#1072
  end
  object Label4: TLabel
    Left = 8
    Top = 256
    Width = 38
    Height = 13
    Caption = #1043#1088#1091#1087#1087#1099
  end
  object Label5: TLabel
    Left = 8
    Top = 304
    Width = 54
    Height = 13
    Caption = #1048#1089#1090#1086#1095#1085#1080#1082#1080
  end
  object btnAccept: TBitBtn
    Left = 120
    Top = 408
    Width = 81
    Height = 25
    Caption = #1055#1088#1080#1085#1103#1090#1100
    TabOrder = 2
    OnClick = btnAcceptClick
    Kind = bkOK
  end
  object btnCancel: TBitBtn
    Left = 208
    Top = 408
    Width = 81
    Height = 25
    Caption = #1054#1090#1084#1077#1085#1080#1090#1100
    TabOrder = 3
    OnClick = btnCancelClick
    Kind = bkCancel
  end
  object rgLife: TRadioGroup
    Left = 8
    Top = 8
    Width = 137
    Height = 97
    Items.Strings = (
      #1074#1089#1077
      #1090#1086#1083#1100#1082#1086' '#1078#1080#1074#1099#1077
      #1090#1086#1083#1100#1082#1086' '#1091#1084#1077#1088#1096#1080#1077
      #1074' '#1078#1080#1074#1099#1093' '#1076#1086)
    TabOrder = 0
    OnClick = rgLifeClick
  end
  object edName: TComboBox
    Left = 8
    Top = 176
    Width = 281
    Height = 21
    ItemHeight = 13
    Sorted = True
    TabOrder = 1
    Text = '*'
  end
  object rgSex: TRadioGroup
    Left = 152
    Top = 8
    Width = 137
    Height = 97
    Items.Strings = (
      #1074#1089#1077
      #1090#1086#1083#1100#1082#1086' '#1084#1091#1078#1095#1080#1085#1099
      #1090#1086#1083#1100#1082#1086' '#1078#1077#1085#1097#1080#1085#1099)
    TabOrder = 4
  end
  object edAliveBeforeDate: TMaskEdit
    Left = 8
    Top = 128
    Width = 137
    Height = 21
    Enabled = False
    EditMask = '!99/99/9999;1;_'
    MaxLength = 10
    TabOrder = 5
    Text = '  .  .    '
  end
  object GroupBox1: TGroupBox
    Left = 8
    Top = 352
    Width = 281
    Height = 41
    Caption = #1056#1072#1089#1096#1080#1088#1077#1085#1085#1099#1077
    TabOrder = 6
    object CheckPatriarch: TCheckBox
      Left = 8
      Top = 16
      Width = 185
      Height = 17
      Caption = #1058#1086#1083#1100#1082#1086' '#1075#1083#1072#1074#1099' '#1089#1077#1084#1077#1081
      TabOrder = 0
    end
  end
  object cbResidence: TComboBox
    Left = 8
    Top = 224
    Width = 281
    Height = 21
    ItemHeight = 13
    Sorted = True
    TabOrder = 7
    Text = '*'
  end
  object cbGroup: TComboBox
    Left = 8
    Top = 272
    Width = 281
    Height = 21
    Style = csDropDownList
    ItemHeight = 13
    TabOrder = 8
  end
  object cbSource: TComboBox
    Left = 8
    Top = 320
    Width = 281
    Height = 21
    Style = csDropDownList
    ItemHeight = 13
    TabOrder = 9
  end
end
