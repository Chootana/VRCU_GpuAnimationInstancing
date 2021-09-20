# Usage - 歩行スポーン

GPU Instancing対応させたキャラクターを用いて歩行モーションをランダムスポーンさせる方法について述べる．

歩行などのモーションはAnimation Clipが繰り返されるたびに全体が移動するだろう．それに対応させるために

BOOTHでサンプル付きを購入された方は既にセットアップ済みのSceneがある
- AnimGPUInstancing/Sample/Scenes/Locomotion.unityのシーンを開いて実行すると確認できる．

![locomotion_spawn_001](https://user-images.githubusercontent.com/44863813/134013760-7d80e13f-5d22-4a7d-9819-4c5fb8f9a093.gif)
（100体クローン，位置・回転・スケールのランダム設定，1つのアニメーションランダム再生，1つのアニメーションを20回繰り返したら初期位置まで戻る）

## インスタンシングしたいキャラクターを準備する
- [基本操作](usage_basic.md)参照

- マテリアル設定について確認する
  
  ![image](https://user-images.githubusercontent.com/44863813/134015008-bff22615-14d2-4822-89be-7baa16c80e9f.png)

  - Apply Root Motionにチェックを入れると，残りの設定欄が開く
    - Repeat Start Texture: Repeat Textureを参照する位置を設定する．Animation Frame Info ListのVector4.zを入力する
    - Repeat Max: テクスチャに書き込んだ最大リピート回数．Animation Frame Info ListのVector4.wを入力する
    - Repeat Num: アニメーションの実際のリピート回数

## AGI_SpawnRandom.prefabをHierarchyに置く
![image](https://user-images.githubusercontent.com/44863813/134013948-6cce2f1e-e030-4d75-977b-0fc2cafc5dd3.png)


### パラメータ説明

- General Parameters 
  - Anim Character: 指定したキャラクターを設定する
  - Animation Frame Info List: 指定したキャラクターを設定する

-  Random Spawn
   -  Spawn Num: スポーンさせる数
   -  Random X : スポーン範囲（X方向）
      -  10ならばX座標方向-10 ~ 10の範囲にスポーンする　 
   -  Random Y: スポーン範囲（Y方向）
   -  Random Z: スポーン範囲（Z方向）
   -  Random Rotation: 回転範囲（Y方向）
      -  30ならばY方向周りに-30~30度回転してスポーンする

- Apply Root Motion for Locomotion （今回使用する）
  - Apply Root Motion: チェックをいれるとルートモーションが適用される
  - Repeat Num: アニメーションの実際のリピート回数


## 動作確認
- 実行するかVRChatのワールドとしてアップロード確認できる．
