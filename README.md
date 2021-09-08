# VRCU_GpuAnimationInstancing

![gif_animation_003](https://user-images.githubusercontent.com/44863813/131253241-79397313-9fe3-4cd4-bf86-10806afd6720.gif)

# TODO 
- Various Shader 
  - now we only make for Unlit/Surface without Shadow 

- How to add Blend Shape info to Mesh 

- How to controll more logically 
  - animation texture + controller texture (custome render texture)

- How to proceed if there are 2 or more skinned mesh 
- How to proceed if there are 2 mat 


- Shadow切り替え，Lighting切り替え
- 切り替えによってShaderの処理を軽くする方法
- 複数マテリアル対応
- TextureなしColorのみマテリアル対応
- Boundsの自動計算

- StartFrameの表記バグ（Loop対応）

# UI設計
- PrefabをDD 
  - Skinned Mesh Rendererが1つだけ確認
  - Animator確認
    - Animationのリストアップ
  - Materialの数確認

- それぞれのAnimationに対して
  - 入れるかどうかチェック
  - 最大Loop数の入力

- Animさせたいタイプ
  - ランダム（複数のClipをランダムに持たせて繰り返させたい）
  - ランダム + 連続　(複数のClipを持つが順番に繰り返させたい)
  - Clipは1, 2つだけ，歩行系，Loopするのが基本系
    - ここがMECEじゃない．Udon側の書き方含めて，パターンを洗い出して

- Path表示
- 保存名の提案・修正
- Shader選択

- GENERATE ボタン
- やり直しは要らないかも


  