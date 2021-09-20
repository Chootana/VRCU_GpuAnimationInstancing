# Tips

## SkinnedMeshRenderの数について 
- 1つのキャラクターに対して1つのSkinnedMeshRenderが推奨
- 複数のSMRを持つアバターでも変換可能だが，その分負荷は大きくなる


## 用意してるShader説明 
- Base: Unlit風
  - 一番軽い．お試しとしても
- Base_Shader: Unlit風 + Cast Shadow対応（SetPass Callsが増える）
  - キャラクターの足元に影が描画される．100体出しても負荷的に気にならないのでおすすめ


## カスタムシェーダー
- 独自にエフェクトをかけたい人向け
  - AnimGPUInstancing/Shadersフォルダにshaderを置くことで，エディタ拡張側でShaderを選択できるようになる．


# できないこと(WIP)
- VRCInstancingのランダムスポーンはローカルのためプレイヤーごとに同期していない
- 特定スポーンなら位置とアニメーションの種類はある程度制御できるが完全ではない
- Boundsが固定（Mesh自体は動いていないため）．シューティングやバトル用のモブキャラには非対応