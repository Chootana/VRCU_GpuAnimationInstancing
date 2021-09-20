# Note 
ここには各種noteを書きます

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