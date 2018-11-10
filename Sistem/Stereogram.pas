unit Stereogram;

 (*

     TStereogram ver. 1.0 (Freeware)
     (c) Copyright 1999      Andrej Olejnik

     email olej@atlas.cz  or olej@asset.sk
     Any comments and BUG reports are welcome.

     TStereogram create fast Single Image Random Dot Stereograms (SIRDS),
     or Single Image Stereograms (SIS) depending on whether the picture
     contains random dots as a base for the 3D effect, or a repetitive
     image pattern. Unfortunately, each commercial company has labelled
     them differently. Shop owners generally don't know what you mean,
     unless you say "Hollusion" or one of the many other specific names.

     See some descriptions on WWW - http://www.ucl.ac.uk/~ucapwas/stech.html

     This unit & sample is Freeware.
     for use with Delphi 3/4  (I don't try with Delphi 2.0)
     this component is for private use only,
     You must not sell the component or the source code


     TStereogram uses TFastDIB v 2.5 from
     Gordon Alex Cowie <gfody@jps.net>
     www.jps.net/gfody


     TStereogram implements two algorithms

     1. Simple random algorithm
        used in sgtFastRandomDot and sgtColoredDot stereogram types

        This is fast asymmetric algorithm drawing from left to right
        NO Hidden-surface removal
        with complicated images raises echoes


     2. Slower (50% then first) but faithful algorithm
        used in sgtRandomDotTextured and sgtTextured types

        Fully symmetric algorithm for perfect image geometry
        Hidden-surface removal
        Absence of artifactual 'echoes'
        Oversampling
        From-the-centre-outwards pattern application




       To Start generating call  Generate
       result bitmap is saved in ResultMap and FastResultMap


      Property description:


      property ResultMap: TPicture read ReadResultMap;

        After Generate can get result Bitmap


      property FastResultMap: TFastDIB read FFastResultMap;

        Like ResultMap, but result is TFastDIB


      property OnGenerate: TNotifyEvent read FOnGenerate write FOnGenerate;

        Enable set Generate event and display progress


      property DepthMap: TPicture read FDepthMap write SetDepthMap;

        You must set DepthMap, whitch contains gray bitmap
        lighter points are further, darker are far


      property Height: Integer read GetHeight write SetHeight default 600;

        Height of result bitmap


      property MaskMap: TPicture read FMaskMap write SetMaskMap;

        Bitmap used to texture in SIS stereograms


      property Separation: Integer read GetSeparation write SetSeparation default 90;

        Maximum distance between two points, which represents maximal depth


      property StereoType: TStereoGramType read GetType write SetType;

        sgtFastRandomDot - Fast random dot algorithm, don't use MapMask
                           Dot can be Black & White or RGB - See RandomDotColor
                           Dot Intensity B&W sets with DotIntensity


        sgtColoredDot -    Fast random dot algorithm, MaskMap used
                           to colorize background

        sgtRandomDotTextured - Exact algorithm with similar results like
                               sgtFastRandomDot,  random dot texture
                               is prepared first in MaskMap, then is image
                               compute like with real texture

        sgtTextured  -      Exact algorithm, MapMask is used to texture
                            result image, use Oversampling for smooth images,
                            this gets nice fast results

      property Width: Integer read GetWidth write SetWidth default 800;

        Width of result bitmap


      property DotIntensity: Integer read FDotIntensity write SetDotIntensity default 50;

        Random dot intensity in percent used for generating SIRDs
        Random image stereograms


      property DPI: Integer read FDPI write SetDPI default 72;

        Display device resolution


      property Blur: TBlurType read FBlur write FBlur;

        Good for random dot stereograms


      property RandomDotColor: TRandomDotColorType read FRandomDotColor write FRandomDotColor;

        Type of random dot - black & white  or  RGB


      property Oversampling: Integer read Foversam write Setoversam;

        For smooth results use Oversapling with Textured & RandomDotTexturef


      property ObservDistance: Double read FobsDist write SetobsDist;

        Observer distance from canvas/screen in inches


      property Max3Ddepth: Double read Fmaxdepth write Setmaxdepth;

        Maximal distance from canvas/screen in inches for projected 3D object


      property Min3Ddepth: Double read Fmindepth write Setmindepth;

        Minimal distance from canvas/screen in inches for projected 3D object
 *)



interface

uses WinTypes, WinProcs, Messages, SysUtils, Classes, Controls,
  Forms, Graphics, FastDIBOlej, ExtCtrls;


const

  MaxWidth = 1024; {maximum width of stereogram picture}
  MaxHeight = 800; {maximum height of stereogram picture}
  MaxOversamp = 8;

  {Maximal width * max oversampling}
  maxX = MaxWidth * MaxOversamp;

  DepthLevels = 256; {From 128 no visible changes}

type


  TStereoGramType = (sgtFastRandomDot, sgtColoredDot, sgtRandomDotTextured, sgtTextured);
  TBlurType = (blurNone, blurLight, blurMedium, blurHeavy);
  TRandomDotColorType = (dctBlackWhite, dctRGB);

  TStereogram = class(TComponent)
  private

    patHeight,
      xdpi,
      ydpi,
      yShift: Integer;

    lastlinked,
      i,
      vwidth,
      eyeSep,
      veyeSep,
      maxsep,
      vmaxsep,
      s, k, kk,
      poffset,
      featureZ,
      sep,
      left, right: Integer;

    vis: Boolean;
    red, green, blue: Integer;
    obsDist, mindepth, maxdepth: double;
    hCol: Integer;


    {Array for remember pixel family}
    lookL,
      lookR: array[0..maxx] of word;

    {result color for line}
    colour: array[0..maxx] of TCOLOR;

    SeparTable: array[0..DepthLevels] of Integer;

      { Private fields of TStereoGram }
        { Storage for property DepthMap }
    FDepthMap: TPicture;
        { Storage for property Height }
    FHeight: Integer;
        { Storage for property MaskMap }
    FMaskMap: TPicture;
        { Storage for property ResultMap }
    FResultMap: TPicture;
        { Storage for property Separation }
    FSeparation: Integer;
        { Storage for property Type }
    FType: TStereoGramType;
        { Storage for property Width }
    FWidth: Integer;
        { Pointer to application's OnGenerate handler, if any }
    FOnGenerate: TNotifyEvent;

    FDotIntensity: integer;
    FRandomDotColor: TRandomDotColorType;

    FobsDist: Double;

    Fmaxdepth: Double;

    Fmindepth: Double;

    FOversam: Integer;

    FBlur: TBlurType;
    FBlurParam: Integer;
    FDPI: Integer;

    {FastDIBs for fast access to pixels}
    FFastDepthMap,
      FFastMaskMap,
      FFastResultMap: TFastDIBOlej;

    procedure AutoInitialize;
        { Method to free any objects created by AutoInitialize }
    procedure AutoDestroy;
        { Write method for property DepthMap }
    procedure SetDepthMap(Value: TPicture);
        { Read method for property Height }
    function GetHeight: Integer;
        { Write method for property Height }
    procedure SetHeight(Value: Integer);
        { Write method for property MaskMap }
    procedure SetMaskMap(Value: TPicture);
        { Read method for property Separation }
    function GetSeparation: Integer;
        { Write method for property Separation }

    procedure SetSeparation(Value: Integer);

    procedure SetDPI(Value: Integer);

    procedure Setoversam(Value: Integer);
        { Read method for property Type }
    function GetType: TStereoGramType;
        { Write method for property Type }
    procedure SetType(Value: TStereoGramType);
        { Read method for property Width }
    function GetWidth: Integer;
        { Write method for property Width }
    procedure SetWidth(Value: Integer);

    procedure SetDotIntensity(Value: Integer);
    procedure SetObsDist(Value: Double);
    procedure SetMaxdepth(Value: Double);
    procedure SetMindepth(Value: Double);
    procedure PrepareParams;
    procedure PrepareImages;
    procedure ReturnResulTPicture;
    function GetColor(FBMP: TFastDIBOlej; Col, Row: Integer): TColor;
    procedure SetColor(FBMP: TFastDIBOlej; Col, Row: Integer; R, G, B: Byte);

    procedure PrepareSeparTable;
    procedure PrepareRandomTexture;
    procedure GenerateStereogram;
    function ReadResultMap: TPicture;

    procedure MakeLineRandomDot(Row: Integer);
    procedure MakeLineColoredDot(Row: Integer);
    procedure MakeLineTextured(Row: Integer);

  protected

      { Protected methods of TStereoGram }
    procedure Loaded; override;

  public
    Progress: Integer; {Progress in percents}

    property ResultMap: TPicture read ReadResultMap; {Stored result after Generate}
    property FastResultMap: TFastDIBOlej read FFastResultMap; {Stored result after Generate}

      { Public methods of TStereoGram }
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;


    {Starts generate  - result bitmap is saved in ResultMap and FastResultMap}
    procedure Generate;

  published
      { Published properties of TStereoGram }
    property OnGenerate: TNotifyEvent read FOnGenerate write FOnGenerate;
    property DepthMap: TPicture read FDepthMap write SetDepthMap;
    property Height: Integer read GetHeight write SetHeight default 600;
    property MaskMap: TPicture read FMaskMap write SetMaskMap;
    property Separation: Integer read GetSeparation write SetSeparation default 90;
    property StereoType: TStereoGramType read GetType write SetType;
    property Width: Integer read GetWidth write SetWidth default 800;
    property DotIntensity: Integer read FDotIntensity write SetDotIntensity default 50;
    property DPI: Integer read FDPI write SetDPI default 72;
    property Blur: TBlurType read FBlur write FBlur;
    property RandomDotColor: TRandomDotColorType read FRandomDotColor write FRandomDotColor;
    property Oversampling: Integer read Foversam write Setoversam;
    property ObservDistance: Double read FobsDist write SetobsDist;
    property Max3Ddepth: Double read Fmaxdepth write Setmaxdepth;
    property Min3Ddepth: Double read Fmindepth write Setmindepth;

  end;

procedure Register;

implementation

procedure Register;
begin
  RegisterComponents('Samples', [TStereogram]);
end;

{ Method to set variable and property values and create objects }

procedure TStereoGram.AutoInitialize;
begin
  Progress := 0;
  FDepthMap := TPicture.Create;
  FMaskMap := TPicture.Create;
  FResultMap := TPicture.Create;
  FFastResultMap := TFastDIBOlej.Create;
  FHeight := 600;
  FWidth := 800;
  FDotIntensity := 50;
  FRandomDotColor := dctBlackWhite;
  FDPI := 72;
  FBlur := blurNone;
  Foversam := 1;
  FobsDist := 14;
  Fmaxdepth := 14;
  Fmindepth := 7;

  PrepareParams;

end; { of AutoInitialize }

{ Method to free any objects created by AutoInitialize }

procedure TStereoGram.AutoDestroy;
begin
  FDepthMap.Free;
  FMaskMap.Free;
  FResultMap.Free;
  FFastResultMap.Free;
end; { of AutoDestroy }


procedure TStereoGram.SetDepthMap(Value: TPicture);
begin
     { Use Assign method because TPicture is an object type }
  FDepthMap.Assign(Value);
  if (Width < DepthMap.Width) then Width := DepthMap.Width;
  if (Height < DepthMap.height) then Height := DepthMap.Height;
end;

{ Read method for property Height }

function TStereoGram.GetHeight: Integer;
begin
  Result := FHeight;
end;

{ Write method for property Height }

procedure TStereoGram.SetHeight(Value: Integer);
begin
  if value < 1 then FHeight := 600
  else
    if Value > MaxHeight then FHeight := MaxHeight
    else FHeight := Value;
  if FType = sgtColoredDot then
  begin
    FHeight := DepthMap.Height;
  end;

end;

{ Write method for property MaskMap }

procedure TStereoGram.SetMaskMap(Value: TPicture);
begin
  FMaskMap.Assign(Value);
end;

{ Read method for property Separation }
function TStereoGram.GetSeparation: Integer;
begin
  Result := FSeparation;
end;

{ Write method for property Separation }

procedure TStereoGram.SetSeparation(Value: Integer);
begin
  FMaxDepth := ((-1 * Value * FDPI * FobsDist) / (Value - (Trunc(FDPI * 2.5))) + 1) / FDPI;
  Fmaxdepth := (Trunc(Fmaxdepth * 1000)) / 1000;
  if Fmindepth > FMaxDepth then Fmindepth := Fmaxdepth - 5;
  PrepareParams;
end;


{Prepare separation values to table used for faster resuls}
procedure TStereoGram.PrepareSeparTable;
var
  x: Integer;
  featureZ: Integer;
begin
  for x := 0 to DepthLevels - 1 do
  begin
    featureZ := Trunc(maxdepth - x * (maxdepth - mindepth) / DepthLevels);
    SeparTable[x] := Round((veyeSep * featureZ) / (featureZ + obsDist));
  end;
end;


procedure TStereoGram.Setoversam(Value: Integer);
begin
  if Value < 1 then FOversam := 1
  else
    if Value > MaxOversamp then FOversam := MaxOversamp
    else
      FOversam := Value;
  if (StereoType = sgtFastRandomDot) or (StereoType = sgtColoredDot) then
    FOversam := 1;
end;


procedure TStereoGram.SetObsDist(Value: Double);
begin
  FobsDist := Value;
  PrepareParams;
end;

procedure TStereoGram.SetMaxdepth(Value: Double);
begin
  FmaxDepth := Value;
  PrepareParams;
end;

procedure TStereoGram.SetMindepth(Value: Double);
begin
  FminDepth := Value;
  PrepareParams;
end;



{ Read method for property Type }

function TStereoGram.GetType: TStereoGramType;
begin
  Result := FType;
end;

{ Write method for property Type }

procedure TStereoGram.SetType(Value: TStereoGramType);
begin
  if Value = sgtFastRandomDot then FOversam := 1;
  if Value = sgtColoredDot then
  begin
    Width := DepthMap.Width;
    Height := DepthMap.Height;
  end;
  FType := Value;
end;



{ Read method for property Width }

function TStereoGram.GetWidth: Integer;
begin
  Result := FWidth;
end;

{ Write method for property Width }

procedure TStereoGram.SetWidth(Value: Integer);
begin
  if Value < 1 then FWidth := 600
  else if Value > MaxWidth then FWidth := MaxWidth else
    FWidth := Value;
  if FType = sgtColoredDot then
  begin
    FWidth := DepthMap.Width;
  end;
end;

procedure TStereoGram.SetDPI(Value: Integer);
begin
  if Value < 10 then FDPI := 10
  else FDPI := Value;

  if (FDPI > Width) then
  begin
    FDPI := Width div 2;
  end;
  if (FDPI > Height) then
  begin
    FDPI := Height div 2;
  end;
  PrepareParams;
end;

procedure TStereoGram.SetDotIntensity(Value: Integer);
begin
  if Value < 1 then FDotIntensity := 1
  else
    if Value > 99 then FDotIntensity := 99
    else FDotIntensity := Value;
end;

{ Method to generate OnGenerate event }

procedure TStereoGram.PrepareParams;
begin

  patHeight := FMaskMap.Height;
  xdpi := FDPI;
  ydpi := FDPI;
  yShift := ydpi div 16;
  vwidth := width * Foversam;
  obsDist := FDPI * FobsDist;
  eyeSep := Round(FDPI * 2.5);
  veyeSep := eyeSep * Foversam;
//  maxdepth := xdpi * 12;
  maxdepth := FDPI * FMaxDepth;
  maxsep := Trunc(((eyeSep * maxdepth) / (maxdepth + obsDist))); // pattern must be at
                                                                  // least this wide

    {If maxSep is greater then map bitmap}
  if (maxSep > FMaskMap.Width) and
    (FMaskMap.Width > 0) and ((FType = sgtRandomDotTextured) or (FType = sgtTextured))
    then maxSep := FMaskMap.Width;

  FSeparation := MaxSep;

  if FType = sgtRandomDotTextured then
    patHeight := MaxSep;

  vmaxsep := Foversam * maxsep;
  s := vwidth div 2 - vmaxsep div 2;
  poffset := vmaxsep - (s mod vmaxsep);
//  mindepth := Trunc((sepfactor * maxdepth * obsdist) / ((1 - sepfactor) * maxdepth + obsdist));
  mindepth := FDPI * FMinDepth;

  K := (Width - DepthMap.Width) div 2;
  KK := (Height - DepthMap.Height) div 2;


  {BLUR CONST}
  if FBlur = blurLight then FBlurParam := 15;
  if FBlur = blurMedium then FBlurParam := 10;
  if FBlur = blurHeavy then FBlurParam := 7;

end;


procedure TStereoGram.PrepareImages;
begin
  FFastResultMap.Free;
  FFastResultMap := TFastDIBOlej.Create;
  FFastDepthMap := TFastDIBOlej.Create;
  FFastMaskMap := TFastDIBOlej.Create;
  FFastResultMap.LoadFromHandle(HGDIOBJ(FResultMap.Bitmap.Handle), 24, 0);
  FFastDepthMap.LoadFromHandle(HGDIOBJ(FDepthMap.Bitmap.Handle), 24, 0);
  FFastMaskMap.LoadFromHandle(HGDIOBJ(FMaskMap.Bitmap.Handle), 24, 0);
end;

procedure TStereoGram.ReturnResulTPicture;
begin
//  FFastResultMap.Draw(FResultMap.Canvas.Handle, 0, 0);

  FFastDepthMap.Free;
  FFastMaskMap.Free;
//  FFastResultMap.Free;
end;

function TStereoGram.GetColor(FBMP: TFastDIBOlej; Col, Row: Integer): TColor;
begin
(*
 {Test for debug error calls}
  if Col + 1 > FBmp.Width then ShowMessage('Error');
  if Row + 1 > FBmp.Height then ShowMessage('Error');
*)

  Result := (FBMP.Pixels[Row, Col].B shl 16) +
    (FBMP.Pixels[Row, Col].G shl 8) +
    (FBMP.Pixels[Row, Col].R);
end;

procedure TStereoGram.SetColor(FBMP: TFastDIBOlej; Col, Row: Integer; R, G, B: Byte);
var
  FAF: TFColor;
begin

(*
 {Test for debug error calls}
  if Col + 1 > FBmp.Width then ShowMessage('Error');
  if Row + 1 > FBmp.Height then ShowMessage('Error');
*)
  Faf.r := R;
  Faf.g := G;
  Faf.b := B;
  FBMP.Pixels24[Row, Col] := Faf;
end;


procedure TStereoGram.Generate;
begin
  if (Progress > 0) and (Progress < 100) then Exit;
 {Test bitmaps}
  if FDepthMap.Bitmap.Empty then
    raise Exception.Create('DepthMap is empty');

  if (FType = sgtColoredDot) or (FType = sgtTextured) then
  begin
    if FMaskMap.Bitmap.Empty then
      raise Exception.Create('MaskMap is empty');
  end;
  if (FType = sgtColoredDot) then
  begin
    if (FDepthMap.Width <> FMaskMap.Width) or
      (FDepthMap.Height <> FMaskMap.Height) then
      raise Exception.Create('If Use Colored dot, MaskMap & DepthMap must have same size');
  end;
  if (FType = sgtColoredDot) then
  begin
    if (FDepthMap.Width <> Width) or
      (FDepthMap.Height <> Height) then
      raise Exception.Create('If Use Colored dot, DepthMap & Result Image must have same size');
  end;
  ResultMap.Bitmap.Width := Width;
  ResultMap.Bitmap.Height := Height;
  ResultMap.Bitmap.Monochrome := False;
  {Main steps to get result image}
  PrepareParams;
  PrepareImages;
  PrepareSeparTable;
  GenerateStereogram;
  ReturnResulTPicture;
end;


function TStereoGram.ReadResultMap: TPicture;
begin
  FFastResultMap.Draw(FResultMap.Bitmap.Canvas.Handle, 0, 0);
  Result := FResultMap;
end;

procedure TStereoGram.MakeLineRandomDot(Row: Integer);
var
  x, y: Integer;
  kkk: Integer;
begin

  y := Row;
  for x := 0 to Width do LookL[x] := x;

  for x := 0 to Width - 1 do
  begin

    if (Y <= KK) or (Y >= (FDepthMap.Height + KK)) or
      ((X div FOversam) <= K) or ((X div FOversam) >= (FDepthMap.Width + K))
      then
    begin
      hCol := 0;
    end
    else
    begin
      hCol := Trunc(((GetColor(FFastDepthMap, x div Foversam - k, y - kk) / $00FFFFFF) * (DepthLevels - 1)));
    end;

    if FBlur = blurNone then
    begin
      Sep := SeparTable[hCol];
    end
    else
    begin
      featureZ := Trunc(maxdepth - hCol * (maxdepth - mindepth) / DepthLevels);
      sep := Round((veyeSep * featureZ) / (featureZ + obsDist) + ((random(10) - 5) / FBlurParam));
    end;

    left := x - (sep div 2);
    right := left + sep;
    if (0 <= left) and (right < Width) then
    begin
      kkk := LookL[left];
      while (kkk <> left) and (kkk <> right) do
      begin
        if (kkk < right) then
        begin
          left := kkk;
        end
        else
        begin
          left := right;
          right := kkk;
        end;
        kkk := LookL[Left];
      end;
      LookL[left] := right;
    end;
  end;

  for x := Width - 1 downto 0 do
  begin
    if (LookL[x] = x) then
    begin
      if FRandomDotColor = dctBlackWhite then
      begin
        if random(100) > FDotIntensity then
        begin
          SetColor(FFastResultMap, x, y,
            255, 255, 255);
        end
        else
        begin
          SetColor(FFastResultMap, x, y,
            0, 0, 0);
        end;
      end
      else
      begin
        SetColor(FFastResultMap, x, y,
          Random(4) * 64 + 64, Random(4) * 64 + 64, Random(4) * 64 + 64);
      end;

    end
    else
    begin
      FFastResultMap.Pixels[y, x] := FFastResultMap.Pixels[y, LookL[x]];
    end;
  end;
end;



procedure TStereoGram.MakeLineColoredDot(Row: Integer);
var
  x, y: Integer;
  kkk: Integer;
  Faf: TFColor;
begin

  y := Row;
  for x := 0 to Width do LookL[x] := x;

  for x := 0 to Width - 1 do
  begin

    if (Y <= KK) or (Y >= (FDepthMap.Height + KK)) or
      ((X div FOversam) <= K) or ((X div FOversam) >= (FDepthMap.Width + K))
      then
    begin
      hCol := 0;
    end
    else
    begin
      hCol := Trunc(((GetColor(FFastDepthMap, x div Foversam - k, y - kk) / $00FFFFFF) * (DepthLevels - 1)));
    end;


    if FBlur = blurNone then
    begin
      Sep := SeparTable[hCol];
    end
    else
    begin
      featureZ := Trunc(maxdepth - hCol * (maxdepth - mindepth) / DepthLevels);
      sep := Round((veyeSep * featureZ) / (featureZ + obsDist) + ((random(10) - 5) / FBlurParam));
    end;


    left := x - (sep div 2);
    right := left + sep;
    if (0 <= left) and (right < Width) then
    begin
      kkk := LookL[left];
      while (kkk <> left) and (kkk <> right) do
      begin
        if (kkk < right) then
        begin
          left := kkk;
        end
        else
        begin
          left := right;
          right := kkk;
        end;
        kkk := LookL[Left];
      end;
      LookL[left] := right;

      {-----}
      if GetColor(FFastMaskMap, x, y) <> 0 then
      begin
        (*
        ZColor[left] := ZColor[x];
        ZColor[x] := 0;
        *)
        if Random(2) = 1 then
        begin


          FFastMaskMap.Pixels[y, left] := FFastMaskMap.Pixels[y, x];
        end
        else
        begin
          FFastMaskMap.Pixels[y, x] := FFastMaskMap.Pixels[y, left];
        end;
      end;
    end;
  end;

  for x := Width - 1 downto 0 do
  begin
    if (LookL[x] = x) then
    begin
      if FRandomDotColor = dctBlackWhite then
      begin
        if random(100) > FDotIntensity then
        begin
          SetColor(FFastResultMap, x, y,
            255, 255, 255);
        end
        else
        begin
          SetColor(FFastResultMap, x, y,
            0, 0, 0);
        end;
      end
      else
      begin
        SetColor(FFastResultMap, x, y,
          Random(4) * 64 + 64, Random(4) * 64 + 64, Random(4) * 64 + 64);
      end;

    end
    else
    begin
      FFastResultMap.Pixels[y, x] := FFastResultMap.Pixels[y, LookL[x]];
    end;

    if (GetColor(FFastMaskMap, x, y) <> 0) (* and (ZOut[x] = 0)  *) then
    begin
      if random((10 - DotIntensity div 10 + 5) + 1) = 1 then
      begin
        if random(2) = 1 then
        begin
          FFastResultMap.Pixels[y, x] := FFastMaskMap.Pixels[y, x];
          FFastResultMap.Pixels[y, LookL[x]] := FFastMaskMap.Pixels[y, x];
        end
        else
        begin
          FFastResultMap.Pixels[y, x] := FFastMaskMap.Pixels[y, LookL[x]];
          FFastResultMap.Pixels[y, LookL[x]] := FFastMaskMap.Pixels[y, LookL[x]];
        end;
      end;
    end;
  {--------}
  end;
end;


procedure TStereoGram.MakeLineTextured(Row: Integer);
var
  x, xx, Y: integer;
  col: TColor;
begin

  y := Row;

  for x := 0 to vwidth - 1 do
  begin
    lookL[x] := x;
    lookR[x] := x;
  end;

  for x := 0 to vwidth - 1 do
  begin
    if ((x mod Foversam) = 0) then // SPEEDUP for oversampled pictures
    begin
      if (Y <= KK) or (Y >= (FDepthMap.Height + KK)) or
        ((X div Foversam) <= K) or ((X div Foversam) >= (FDepthMap.Width + K))
        then
      begin
        hCol := 0;
      end
      else
      begin
        hCol := Trunc(((GetColor(FFastDepthMap, x div Foversam - k, y - kk) / $00FFFFFF) * (DepthLevels - 1)));
      end;
      if FBlur = blurNone then
      begin
        Sep := SeparTable[hCol];
      end
      else
      begin
        featureZ := Trunc(maxdepth - hCol * (maxdepth - mindepth) / DepthLevels);
        sep := Round((veyeSep * featureZ) / (featureZ + obsDist) + ((random(10) - 5) / FBlurParam));
      end;
    end;

    left := x - sep div 2;
    right := left + sep;
    vis := TRUE;
    if ((left >= 0) and (right < vwidth)) then
    begin
      if (lookL[right] <> right) then // right pt already linked
      begin
        if (lookL[right] < left) then // deeper than current
        begin
          lookR[lookL[right]] := lookL[right]; // break old links
          lookL[right] := right;
        end else vis := FALSE;
      end;
      if (lookR[left] <> left) then // left pt already linked
      begin
        if (lookR[left] > right) then // deeper than current
        begin
          lookL[lookR[left]] := lookR[left]; // break old links
          lookR[left] := left;
        end else vis := FALSE;
      end;
      if (vis = TRUE) then
      begin
        lookL[right] := left;
        lookR[left] := right;
      end;
         // make link
    end;
  end;

  lastlinked := -10; // dummy initial value
  for x := s to vwidth - 1 do
  begin
    if ((lookL[x] = x) or (lookL[x] < s)) then
    begin
      if (lastlinked = (x - 1)) then colour[x] := colour[x - 1]
      else
      begin
        colour[x] := GetColor(FFastMaskMap, ((x + poffset) mod vmaxsep) div Foversam,
          (y + ((x - s) div vmaxsep) * yShift) mod patHeight);
      end
    end
    else
    begin
      colour[x] := colour[lookL[x]];
      lastlinked := x; // keep track of the last pixel to be constrained
    end;
  end;

  lastlinked := -10; // dummy initial value
  for x := s - 1 downto 0 do
  begin
    if (lookR[x] = x) then
    begin
      if (lastlinked = (x + 1)) then colour[x] := colour[x + 1]
      else
      begin
        colour[x] := GetColor(FFastMaskMap, ((x + poffset) mod vmaxsep) div Foversam,
          (y + ((s - x) div vmaxsep + 1) * yShift) mod patHeight);
      end
    end
    else
    begin
      colour[x] := colour[lookR[x]];
      lastlinked := x; // keep track of the last pixel to be constrained
    end;
  end;


  x := 0;
  for xx := 0 to (vWidth div Foversam) - 1 do
  begin
    red := 0; green := 0; blue := 0;
   // use average colour of virtual pixels for screen pixel
    i := x;
    while (i < (x + Foversam)) do
    begin
      col := colour[i];
      red := red + col and $000000FF;
      green := green + (col and $0000FF00) shr 8;
      blue := blue + (col and $00FF0000) shr (16);
      i := i + 1;
    end;
    SetColor(FFastResultMap, x div Foversam, y,
      red div Foversam, green div Foversam, blue div Foversam);
    x := x + Foversam;
  end;
end;



procedure TStereoGram.PrepareRandomTexture;
var
  x, xx: Integer;
begin

  FFastMaskMap.Free;
  FFastMaskMap := TFastDIBOlej.Create;
  FFastMaskMap.SetSize(maxsep, FHeight, 24, 0);
  for x := 0 to FFastMaskMap.Width - 1 do
  begin
    for xx := 0 to FFastMaskMap.Height - 1 do
    begin
      if FRandomDotColor = dctBlackWhite then
      begin
        if random(100) > FDotIntensity then
        begin
          SetColor(FFastMaskMap, x, xx,
            255, 255, 255);
        end
        else
        begin
          SetColor(FFastMaskMap, x, xx,
            0, 0, 0);
        end;
      end
      else
      begin
        SetColor(FFastMaskMap, x, xx,
          Random(4) * 64 + 64, Random(4) * 64 + 64, Random(5) * 64 + 64);
      end;
    end;
  end;
end;

procedure TStereoGram.GenerateStereogram;
var
  Y: Integer;

{Begin of main procedure}
begin
//
  Progress := 0;
  Randomize;


  if (FType = sgtRandomDotTextured) then PrepareRandomTexture;
  FFastResultMap.Width := width * Foversam;
  FFastResultMap.Height := Height;

  for Y := 0 to Height - 1 do
  begin
    {Compute line - each type }

    if (FType = sgtFastRandomDot) then MakeLineRandomDot(Y);
    if (FType = sgtColoredDot) then MakeLineColoredDot(Y);
    if (FType = sgtTextured) or (FType = sgtRandomDotTextured) then MakeLineTextured(Y);

    Application.ProcessMessages;
    {Progress}
    Progress := Trunc(Y / (Height / 100));
    if Assigned(FOnGenerate) then
      FOnGenerate(Self);
  end;
  Progress := 100;
  if Assigned(FOnGenerate) then
    FOnGenerate(Self);
end;

constructor TStereoGram.Create(AOwner: TComponent);
begin
  { Call the Create method of the parent class }
  inherited Create(AOwner);
  AutoInitialize;
end;

destructor TStereoGram.Destroy;
begin
  AutoDestroy;
  inherited Destroy;
end;


procedure TStereoGram.Loaded;
begin
  inherited Loaded;
end;


end.

