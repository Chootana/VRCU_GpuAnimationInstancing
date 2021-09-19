# VRC Animation GPU Instancing 

概要を書く
このプロジェクトは〇〇です．

# ターゲットはだれ？？？？？

# DEMO

![gif_animation_003](https://user-images.githubusercontent.com/44863813/131253241-79397313-9fe3-4cd4-bf86-10806afd6720.gif)

# Features 
- GPU instancing対応
- Animation Clipsの複数保持．
- VRC.Instantiateすることで異なるClipsを再生可能
- Quest 対応
- Apply root motion対応．歩行モーションに合わせて全体が移動する
- カスタムShader対応
- BOOTHでセットアップ済みのSCENEを用意．アップロードしてそのまま確認できます．
- 複数SMR対応

# Theory
- ArmatureとVertexとAnimation情報をAnimation Textureに書き込んで保持
- メモリ軽量のためRoot移動は別Repeat Textureに保持
- 二つのTextureをshader側で読み込むことでmeshをshader側で動かす


# Requirement 
- Unity 
- VRC World Ver. 
- Udon Sharp Ver. 

# Usage 

ここも複数のmdに分けよう
1. 基本操作
2. ランダムスポーン 
3. ランダムスポーン(歩行) 
4. 特定位置にスポーン


## SMR -> Animated MRに変換する方法
1. SMR と　Animatorを　用意
2. udon prefabを付ける
3. prefab化する
4. エディタ拡張を開く
5. prefabを選択する
6. Convertする
7. prefabのPathにAnim~~~ができている
8. anim prefabをSceneに置く
9.  Materialの数値をいじる
10. 動いていればOK

## InstancingしてVRCに持ち込む方法
1. AGI_~~~prefabをsceneに置く
2. anim prefab を選択にする
3. パラメータをもろもろいじる
4. VRC Worldなどを置いてUpload 

# Note 

## SMRの数について 
1 SMR 推奨
複数のSMRを持つアバターでも変換可能だが，その分負荷は大きくなる

## 用意してるShader説明 
- Base: Unlit風
- Base_Shader: Unlit風 + Cast Shadow対応（その分負荷が高まります）


## カスタムシェーダー
- カスタムshaderを使用可能
  - AnimGPUInstancing/Shadersフォルダに置く


## できないこと
- VRCInstancingのランダムスポーンはローカルのためプレイヤーごとに同期していない
- 特定スポーンなら位置とアニメーションの種類はある程度制御できるが完全ではない
- Boundsが固定（Mesh自体は動いていないため）．シューティングやバトル用のモブキャラには非対応



# Author 
chootana (ちゅーたな)

twitter: [@choo_zap](https://twitter.com/choo_zap)
# License 
 
VRC Animation GPU Instancing is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).