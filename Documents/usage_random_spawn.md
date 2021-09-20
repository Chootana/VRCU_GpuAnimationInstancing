# Usage - ランダムスポーン

GPU Instancing対応させたキャラクターを用いてランダムスポーンさせる方法について述べる．

BOOTHでサンプル付きを購入された方は既にセットアップ済みのSceneがある
- AnimGPUInstancing/Sample/Scenes/SpawnRandom.unityのシーンを開いて実行すると確認できる．

![usage_random_spawn_003](https://user-images.githubusercontent.com/44863813/134010915-a59af70c-7827-4249-8c4d-0c5e06b1fcc5.gif)
（100体クローン，位置・回転・スケールのランダム設定，3つのアニメーションランダム再生）

## インスタンシングしたいキャラクターを準備する
- [基本操作](Documents/usage_basic.md)参照

## AGI_SpawnRandom.prefab
![image](https://user-images.githubusercontent.com/44863813/134011195-a4e825b2-f04c-440b-855f-57a8064408fa.png)

- AnimGPUInstancing/Prefabs/AGI_SpawnRandom.prefabをHierarchyに置く．
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

- Apply Root Motion for Locomotion （今回は使用しない）
  - Apply Root Motion: チェックをいれるとルートモーションが適用される
  - Repeat Num: ルートモーション含めたアニメーションの回数


## 動作確認
- 実行するかVRChatのワールドとしてアップロード確認できる．
