object fmRecordSelect: TfmRecordSelect
  Left = 548
  Top = 199
  ActiveControl = edFastFilter
  BorderStyle = bsDialog
  Caption = #1042#1099#1073#1086#1088' '#1079#1072#1087#1080#1089#1080
  ClientHeight = 417
  ClientWidth = 385
  Color = clBtnFace
  DefaultMonitor = dmMainForm
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  KeyPreview = True
  OldCreateOrder = True
  Position = poScreenCenter
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnKeyDown = FormKeyDown
  PixelsPerInch = 96
  TextHeight = 13
  object btnSelect: TBitBtn
    Left = 200
    Top = 384
    Width = 81
    Height = 25
    Caption = #1042#1099#1073#1088#1072#1090#1100
    TabOrder = 3
    OnClick = btnSelectClick
    Kind = bkOK
  end
  object btnCreate: TBitBtn
    Left = 104
    Top = 384
    Width = 81
    Height = 25
    Caption = #1044#1086#1073#1072#1074#1080#1090#1100
    TabOrder = 2
    OnClick = btnCreateClick
  end
  object btnCancel: TBitBtn
    Left = 296
    Top = 384
    Width = 81
    Height = 25
    Caption = #1054#1090#1084#1077#1085#1080#1090#1100
    TabOrder = 4
    Kind = bkCancel
  end
  object panList: TPanel
    Left = 0
    Top = 41
    Width = 385
    Height = 329
    Align = alTop
    BevelOuter = bvNone
    BorderWidth = 8
    TabOrder = 1
  end
  object panFilter: TPanel
    Left = 0
    Top = 0
    Width = 385
    Height = 41
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    object edFastFilter: TEdit
      Left = 8
      Top = 8
      Width = 361
      Height = 21
      TabOrder = 0
      OnChange = edFastFilterChange
    end
  end
end
