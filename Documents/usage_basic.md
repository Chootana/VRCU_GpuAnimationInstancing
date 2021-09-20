# Usage - 基本操作

## Animation Mesh Generator (Unityエディタ拡張)を開く
- VRCU_AnimationGPUInstancing.unitypackageをProjectにインストール後，
上記タブの中にあるWindow/Extension Tools/Animation Mesh Generatorをクリックする．

    ![image](https://user-images.githubusercontent.com/44863813/133994399-beb9b4f5-660f-4171-9109-8c2f7de3f7d0.png)

## インスタンシングしたいキャラクターを準備する
1. SkinnedMeshRendererを持ったキャラクターをHierarchyに置く．
  - 今回は[Space Robot Kyle](https://assetstore.unity.com/packages/3d/characters/robots/space-robot-kyle-4696?locale=ja-JP)を使う(使いやすくておススメ)
  - もしprefabしたオブジェクトだったら，"Unpack prefab"しておく． 

2. Animatorを1.のキャラクターに追加する
   - 一番上の階層に追加すればOK

3. Animation Clipsを用意する
   - 今回は[Mixamo](https://www.mixamo.com/)というサイトからアニメーションをダウンロードする（こちらもおススメ）
   - 欲しいモーションを検索し，右側のDOWNLOADをクリック．fbx形式でダウンロードできる．
    
        ![image](https://user-images.githubusercontent.com/44863813/133996027-d377573c-2e85-47d8-808a-2e14595ea9d2.png)
   - 得られたfbxをAssetsに追加してクリックし，Inspectorに表示された設定項目において，"Rig"を"Humanoid"に変更して"Apply"をクリック
    
        ![image](https://user-images.githubusercontent.com/44863813/133996551-434ce391-1c01-47f6-bd6e-9dbb5ad3d093.png)
   
   - fbxの中身を展開し，clipをAnimator側にD&Dする．分かりやすいように名前を変更しておく．
   
   - 複数のアニメーションを想定する場合は，下図のようにする．これらのclipはバラバラに設置してOK
     
        ![image](https://user-images.githubusercontent.com/44863813/133996983-87ea1780-2a28-4b47-b283-1a1985a8dff6.png)

4. AnimationFrameInfoList.prefabを1.のキャラクターに追加する
   - AnimGPUInstancing/Prefabs/AnimationFrameInfoList.prefab
   - 下図のようになってればOK
    
        ![image](https://user-images.githubusercontent.com/44863813/133997471-458ba480-91f4-40be-bf08-330cc2513b6b.png)

5. 1~4で作成したオブジェクトをprefab化させる．
   - Hierarchy側から好きなAssetsのディレクトリにD&Dする．

6. Animation Mesh Generatorウィンドウ内の"Prefab"の中に5で作成したprefabをD&Dする
   - 各種設定項目がでる．1~5に問題があればこの時点で警告が出る．
    
        ![image](https://user-images.githubusercontent.com/44863813/133997805-3d752ef2-d931-42ea-bb51-a28427e095b1.png)

7. 設定を確認後,Convertをクリック
    - 5のディレクトリ内に変換されたprefabやマテリアルなどが作成される．

8. 作成されたRobot Kyle_Anim.prefabをHierarchyにD&Dする．

## 動作確認
- Robot Kyle.prefabを変換したRobot Kyle_Anim.prefabにUdon Behabiourが追加されている．これは4で追加したAnimationFrameInfoListにアニメーションの情報を書き込まれたものである．
- Frame InfoのサイズはAnimation Clipsの数に対応する．
- 各アニメーションの情報がVector4形式で値が格納されている．それぞれ[Start Frame，Frame Count，Repeat Start Frame，Repeat Max]であり，マテリアルの設定で使用する．

    ![image](https://user-images.githubusercontent.com/44863813/133998525-248b656f-5ea6-4742-85ca-b3f3b082e373.png)

- 一つ下の階層のMeshRendererに付随するマテリアルに各種アニメーション用パラメータを与える．今回は１つめのアニメーションの[Start Frame, Frame Count]してみる(Repeat ~~のApply Root Motionする時に必要)

    ![image](https://user-images.githubusercontent.com/44863813/134000330-c282ec53-4484-4c12-96a3-3c62583d34de.png)

- 実行ボタンを押した際にアニメーションが再生されていればOK

    ![robot_basic_3](https://user-images.githubusercontent.com/44863813/134001845-17c29ad5-c98e-4660-b0e7-1abcefbb3339.gif)
