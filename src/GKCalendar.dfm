object fmCalendar: TfmCalendar
  Left = 389
  Top = 105
  BorderStyle = bsToolWindow
  Caption = #1050#1072#1083#1077#1085#1076#1072#1088#1100
  ClientHeight = 313
  ClientWidth = 274
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = True
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object lvDates: TListView
    Left = 8
    Top = 184
    Width = 258
    Height = 121
    Columns = <
      item
        Caption = #1050#1072#1083#1077#1085#1076#1072#1088#1100
        Width = 120
      end
      item
        Caption = #1044#1072#1090#1072
        Width = 120
      end>
    ReadOnly = True
    RowSelect = True
    TabOrder = 1
    ViewStyle = vsReport
  end
  object qtc: TMonthCalendar
    Left = 8
    Top = 8
    Width = 257
    Height = 169
    Date = 40589.483075625000000000
    TabOrder = 0
    WeekNumbers = True
  end
end
