# PredictStarNumberMod

## これは何
[BetterSongListに星予測機能をいれたMod](https://github.com/rakkyo150/BetterSongList-PredictStarNumber)の後継Modです。<br>
今後はこちらを更新していきます。<br>
Standard以外のLawlessなどの譜面にも対応できるようになりました。<br>
![BetterSongListありの星予測値](https://github.com/user-attachments/assets/55b7621f-9aae-4d0f-bd66-9923e5352c7f)

星予測値に基づいた自己ベストのPPを譜面選択画面に表示する機能を追加しました。<br>
![譜面選択画面のbestPP](https://github.com/user-attachments/assets/34024fe0-131d-4175-a35b-40f3a785876a)

~~オリジナルのBetterSongListとともに使用してください。<br>~~
v2.0.0以上又はBeat Saberの本体バージョン1.29.1のためにバックポートしたv1.2.2以上では、BetterSongListが無くても動くようになりました。<br>
もっとも、BetterSongListとともに使用するのを推奨します。<br>
ついでに一応SongDetailsCacheにも依存しないようにしました。<br>
BetterSongListを使用しない場合は以下のように表示されます。<br>
![BetterSongListなしの星予測値](https://github.com/user-attachments/assets/acb0c49e-5b13-4491-9a53-a00b44ba1a8d)

星予測値に基づく自己ベストのPPとプレイ中のスコアに基づくPPをCustom Counter(PredictStarNumberCounter)に表示する機能を追加しました。<br>
![CustomCounter](https://github.com/user-attachments/assets/89acc929-1c64-4c88-9632-1156b83a7c87)

星予測値とそれに基づく自己ベストのPPとプレイ中のスコアに基づくPPをオーバーレイに表示する機能を追加しました。<br>
![オーバーレイ](https://github.com/user-attachments/assets/58bde7c6-74d9-4664-8e6d-bd2c6845aeca)

## 使い方
PredictStarNumberModの設定は、カスタムカウンターとオーバーレイにも影響するので、もし変更する場合は注意してください。<br>
カスタムカウンター(PredictStarNumberCounter)の設定は、Counters+の設定画面から行ってください。<br>
オーバーレイに関しては、[こちら](PredictStarNumberMod\Overlay\README_Overlay.md)を参照してください。<br>
併せて、[beat-saber-overlayのREADME](https://github.com/rynan4818/beat-saber-overlay)を参照してください。<br>

## 依存Mod
カスタムカウンター(PredictStarNumberCounter)は[Counter+](https://github.com/NuggoDEV/CountersPlus)に依存しています。<br>
オーバーレイは[HttpSiraStatus](https://github.com/denpadokei/HttpSiraStatus)に依存しています。<br>
また、[BetterSongList](https://github.com/kinsi55/BeatSaber_BetterSongList)の使用を推奨します。<br>
いずれのModも(特にBetterSongListは)、インストール可能ならば、[ModAssistant](https://github.com/bsmg/ModAssistant)もしくは[BSManager](https://github.com/Zagrios/bs-manager)経由のインストールを推奨します。

## 関連リンク
Training Data : https://github.com/rakkyo150/RankedMapData <br>
Model : https://github.com/rakkyo150/PredictStarNumberHelper <br>
Mod : https://github.com/rakkyo150/PredictStarNumberMod <br>
Chrome Extension : https://github.com/rakkyo150/PredictStarNumberExtension <br>

Counters+ : https://github.com/NuggoDEV/CountersPlus or https://github.com/rakkyo150/CounterPlus-Improved<br>
BetterSongList : https://github.com/kinsi55/BeatSaber_BetterSongList<br>
HttpSiraStatus : https://github.com/denpadokei/HttpSiraStatus<br>
beat-saber-overlay : https://github.com/rynan4818/beat-saber-overlay<br>

```mermaid
flowchart
    First[RankedMapData] -- Training Data --> Second[PredictStarNumberHelper]
    Second -- Learned Model --> PredictStarNumber
    Second -- Learned Model --> Third[PredictStarNumberMod] 
    Second -- Learned Model --> PredictStarNumberExtension
    subgraph Custom Counter
        direction TB
        PredictStarNumberCounter -.- |Dependent| Counter+
    end
    Third --Data--> PredictStarNumberCounter
    subgraph Menu
        direction TB
        BetterSongList
    end
    Third --Modify--> BetterSongList
    subgraph Overlay
        direction TB
        HttpSiraStatus --Data--> beat-saber-overlay
    end
    Third --Data--> HttpSiraStatus
```
