# PredictStarNumberMod

## これは何
[BetterSongListに星予測機能をいれたMod](https://github.com/rakkyo150/BetterSongList-PredictStarNumber)の後継Modです。<br>
オリジナルのBetterSongListとともに使用してください。<br>
また、Standard以外のLawlessなどの譜面にも対応できるようになりました。<br>
今後はこちらを更新していきます。<br>

![スクリーンショット2](https://user-images.githubusercontent.com/86054813/149370978-b97d82a1-ac4a-4268-93e2-817752d37ee0.png)

## 関連リンク
Training Data : https://github.com/rakkyo150/RankedMapData <br>
Model : https://github.com/rakkyo150/PredictStarNumberHelper <br>
Mod : https://github.com/rakkyo150/PredictStarNumberMod <br>
Chrome Extension : https://github.com/rakkyo150/PredictStarNumberExtension <br>

```mermaid
flowchart
    First(RankedMapData) -- Training Data --> Second(PredictStarNumberHelper)
    Second -- Learned Model --> Third(PredictStarNumber)
    Second -- Learned Model --> PredictStarNumberMod
    Second -- Learned Model --> PredictStarNumberExtension
```
