# VRC Animation GPU Instancing (AGI)

## SkinnedMeshRendererをGPU instancing対応させる変換ツール

SkinnedMeshRenderer + Animator -> MeshRenderer + Animation Texture + (専用Shader) に変換することで，使用したいキャラクターとアニメーションをそのままインスタンシング化できる．

<img src="https://user-images.githubusercontent.com/44863813/133965049-96d3a891-808c-4d8c-8f3a-54d47092db81.gif" width="960" title="SKM_MR">


# DEMO

## [ロポリこんちゃん](https://booth.pm/ja/items/1415037)を1000体描画するケース
1. SkinnedMeshRendererのまま1000体描画 -> Draw Callsがおよそ1000程度
<img src="https://user-images.githubusercontent.com/44863813/133979006-106f1484-8380-4bf7-83dc-c38314356e7b.png" width="960" title="smr_statistics">
<img src="https://user-images.githubusercontent.com/44863813/133979371-2180725b-1f4e-4f91-95b9-16011e88b86b.png" width="960" title="smr_debugger">

2. GPU Instancing対応させた後1000体描画 -> Draw Callsはおよそ10程度まで下がり，FPSも大幅に改善させる
<img src="https://user-images.githubusercontent.com/44863813/133979595-1e71ef8b-8ee4-4069-9cc3-7fc3f2981d34.png" width="960" title="agi_statistics">
<img src="https://user-images.githubusercontent.com/44863813/133979647-d4c5b899-97dd-4747-af42-f8f078562cb8.png" width="960" title="agi_debugger">


# Features 
- VRChat対応 (Udon Sharp)
- Quest対応
- Animation Clips 複数対応
- 複数SkinnedMeshRendererを持つキャラクターに対応
- Apply root motion対応（歩行モーションに合わせてアニメーションが移動する）
- BOOTHでセットアップ済みのSAMPLEを用意(VRChat上でそのまま確認できる)

# Theory
- ArmatureとVertexとAnimation情報をAnimation Textureに書き込んで保持
- Root移動の情報は別のRepeat Textureに保持（メモリ軽量化）
- ２つのTextureをshader側で読み込み，vertex shaderでメッシュを動かすことで，アニメーションを再現する

# Requirement 
以下の環境で動作確認済み
- Unity: 2019.4.29f1
- VRCSDK3 World: 2021.08.04.15.07 
- Udon Sharp: 0.20.2

# Usage 
〇〇リンクのunitypackageをインストール
(もしくはBoothからダウンロード)

VRCU_AnimationGPUInstancing.unitypackageをUnity Projectにインストールする．

Extention ToolにAnimationGPUInstancingが現れるのでクリック

使い方は以下を参照

1. [基本操作](Documents/usage_basic.md)
2. [ランダムにスポーンさせる場合](Documents/usage_random_spawn.md)
3. [歩行モーションをスポーンさせる場合](Documents/usage_locomotion_spawn.md)
4. [特定位置にスポーンさせる場合](Documents/usage_points_spawn.md) 


# Tips 
- [その他・注意点](Documents/tips.md)


# Author 
chootana (ちゅーたな)

twitter: [@choo_zap](https://twitter.com/choo_zap)
# License 
 
VRC Animation GPU Instancing is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).