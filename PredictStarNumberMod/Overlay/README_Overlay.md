# オーバーレイとの連携方法

## 使い方
お使いのPCのbeat-saber-overlayのフォルダーにリリースのbeat-saber-overlayのフォルダーの中身を上書きしてください。  
以下の内容をコピーして、`https://github.com/rynan4818/beat-saber-overlay`に準拠したhtml内の`<div id="overlay" class="hidden">`より下の階層のどこかにペーストしてください。
```html
<span id="predicted_star_group">
    <span>（★</span><span id="predicted_star">0.0</span><span>）</span>
</span>
```
以下の内容もコピーして、`https://github.com/rynan4818/beat-saber-overlay`に準拠したhtml内の`<script src="./js/options.js"></script>`の上にペーストしてください。
```html
<script src="./js/predicted_star.js"></script>
```
その上で、[URLパラメータ](https://github.com/rynan4818/beat-saber-overlay#%E3%82%AA%E3%83%97%E3%82%B7%E3%83%A7%E3%83%B3)の`modifiers`に`predicted_star`を追加すれば、表示されるようになります。  
[サンプルのHTML](sample.html)も同梱しているので、そちらも参考にしてください。  

## コピー内容の意味
URLパラメータを追加しない場合の非表示の範囲は`<span id="predicted_star_group">`より下の階層です。  
星予測値は`<span id="predicted_star">0.0</span>`の`0.0`の部分を書き換える処理になっています。  
以上を踏まえて、好みに応じて変更していただくことも可能です。