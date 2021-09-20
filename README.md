# VRC Animation GPU Instancing 

**SkinnedMeshRendererのアニメーションをGPU instancing対応させる変換ツール**

SkinnedMeshRenderer + Animator -> MeshRenderer + Animation Texture + (専用Shader) に変換することで

作成したキャラクターとアニメーションをそのままインスタンシング化できる．

<img src="https://user-images.githubusercontent.com/44863813/133963901-c28aa6c2-53be-4ee5-a11a-485f4589dbef.gif" width="480" title="SKM_MR">


# DEMO
VRChatのワールドでロポリコンちゃんを100体描画した時．

<img src="https://user-images.githubusercontent.com/44863813/131253241-79397313-9fe3-4cd4-bf86-10806afd6720.gif" width="960" title="demo">

# Features 
- VRChat対応 (Udon Sharp)
- Quest対応
- Animation Clips 複数対応
- Apply root motion対応（歩行モーションに合わせて全体が移動する）
- BOOTHでセットアップ済みのSAMPLEを用意(VRChat上でそのまま確認できる)
- 複数SkinnedMeshRendererを持つキャラクターに対応

# Theory
- ArmatureとVertexとAnimation情報をAnimation Textureに書き込んで保持
- メモリ軽量のためRoot移動は別Repeat Textureに保持
- 二つのTextureをshader側で読み込むことでmeshをshader側で動かす

# Requirement 
以下の環境で動作確認している
- Unity: 2019.4.29f1
- VRCSDK3 World: 2021.08.04.15.07 
- Udon Sharp: 0.20.2

# Usage 

## How to install 

1. [基本操作](Documents/usage_basic.md)
2. [ランダムにスポーンさせる場合](Documents/usage_random_spawn.md)
3. [ランダムにスポーンさせる場合（歩行）](Documents/usage_random_spawn_locomotion.md)
4. [特定位置にスポーンさせる場合](Documents/usage_points_spawn.md) 


# Tips 
- [その他・注意点](Documents/tips.md)


# Author 
chootana (ちゅーたな)

twitter: [@choo_zap](https://twitter.com/choo_zap)
# License 
 
VRC Animation GPU Instancing is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).