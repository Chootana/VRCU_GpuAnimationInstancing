# Usage - 指定位置にスポーン

GPU Instancing対応させたキャラクターを用いて指定位置にスポーンさせる方法について述べる．

- スポーン位置やアニメーションをある程度制御した上でAnimation GPU Instancingしたいケースを想定している．
- VRChatのワールド作成に利用する場合はこのケースが多いと思われる


BOOTHでサンプル付きを購入された方は既にセットアップ済みのSceneがある
- AnimGPUInstancing/Sample/Scenes/SpawnPoints.unityのシーンを開いて実行すると確認できる．

![points_spawn_002](https://user-images.githubusercontent.com/44863813/134020415-45f26eed-1f6b-4da8-b4f1-981646ec62b3.gif)
（3つのアニメーション，指定した位置で任意のアニメーションを再生）

## インスタンシングしたいキャラクターを準備する
- [基本操作](usage_basic.md)参照

## スポーン位置を作成する
![image](https://user-images.githubusercontent.com/44863813/134020675-afa708ab-fb7d-45d3-b311-5e2cc0b6624c.png)
- 用意したアニメーションごとに空のGameObjectを作成(Anim 0, Anim 1, Anim 2などと付けることにする)
- それぞれのGameObjectの子供にスポーンさせた位置を表す空のGameObjectを置く
  - 空のGameObjectの位置・回転・スケールがスポーンされるキャラクターに反映される

## AGI_SpawnPoints.prefabをHierarchyに置く
![image](https://user-images.githubusercontent.com/44863813/134021111-ca6a0f6c-27f6-4d44-89c0-05fa202b3f71.png)

### パラメータ説明

- General Parameters 
  - Anim Character: 指定したキャラクターを設定する
  - Animation Frame Info List: 指定したキャラクターを設定する

-  Spawn Points
   -  Spawn Points: サイズはアニメーションの数と合わせる
   -  Elements 0, 1, 2, ...: それぞれに上記で用意したGamaObject群を設定する
      -  ex.) Anim 0 の位置にAnimation 0をするキャラクターをスポーン

- Apply Root Motion for Locomotion 
  - Apply Root Motion: チェックをいれるとルートモーションが適用される
  - Repeat Num: アニメーションの実際のリピート回数


## 動作確認
- 実行するかVRChatのワールドとしてアップロードすることで確認できる．
