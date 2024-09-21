# オーバーレイとの連携方法

## 使い方
お使いのPCのbeat-saber-overlayのフォルダーにリリースのbeat-saber-overlayのフォルダーの中身を上書きしてください。  
以下の内容をコピーして、`https://github.com/rynan4818/beat-saber-overlay`に準拠したhtml内の、少なくとも`<div id="overlay" class="hidden">`より下の階層のどこかにペーストしてください。  
おすすめは、`<div id="performance">`より下の階層のどこかへのペーストです。
```html
<div class="performance_group" id="now_predicted_pp_group">
    <span id="now_predicted_pp_visibility">
        <span class="text">（</span>
        <span class="text" id="now_predicted_pp">0</span>
        <span class="subtext">pp</span>
        <span class="text">）</span>
    </span>
</div>
```
また、以下の内容もコピーして、少なくとも<div id="overlay" class="hidden">より下の階層のどこかにペーストしてください。  
おすすめは、`<div id="meta">`より下の階層のどこかへのペーストです。
```html
<div>
    <span id="predicted_star_mod_group">
        <span id="predicted_star_visibility">
            <span>（★</span><span id="predicted_star">0.0</span><span>）</span>
        </span>
        <span id="best_predicted_pp_visibility">
            <span>（</span><span id="best_predicted_pp">0.0</span><span>pp）</span>
        </span>
    </span>
</div>
```
以下の内容もコピーして、`https://github.com/rynan4818/beat-saber-overlay`に準拠したhtml内の`<script src="./js/options.js"></script>`の上にペーストしてください。
```html
<script src="./js/predicted_star.js"></script>
```
その上で、[URLパラメータ](https://github.com/rynan4818/beat-saber-overlay#%E3%82%AA%E3%83%97%E3%82%B7%E3%83%A7%E3%83%B3)の`modifiers`に`predicted_star`を追加すれば、表示されるようになるはずです。  
[サンプルのHTML](sample.html)も同梱しているので、そちらも参考にしてください。  

## コピー内容の意味
URLパラメータを追加しない場合の非表示の範囲は`<div class="performance_group" id="now_predicted_pp_group">`より下の階層と`<span id="predicted_star_mod_group">`より下の階層です。  
`<span id="now_predicted_pp_visibility">`や`<span id="predicted_star_visibility">`　、`<span id="best_predicted_pp_visibility">`は、いずれも表示非表示をプログラムが自動で切り替えるために必要なタグです。  
リアルタイムの予測ppは`<span class="text" id="now_predicted_pp">0</span>`の`0`の部分を書き換える処理になっています。  
星予測値は`<span id="predicted_star">0.0</span>`の`0.0`の部分を書き換える処理になっています。  
自己ベストの予測ppは`<span id="best_predicted_pp">0.0</span>`の`0.0`の部分を書き換える処理になっています。  
基本的には変更をしないのをおすすめしますが、以上を踏まえて、好みに応じて変更していただくことも可能です。