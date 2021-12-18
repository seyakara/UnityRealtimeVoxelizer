# UnityRealtimeVoxelizer
Unity上で任意の3Dモデルをリアルタイムでボクセル化して描画できるシェーダーです。    
元のモデルのテクスチャおよびベースカラーをボクセル色に反映させることができます。
またボクセルは任意のメッシュで置き換えることができます。

## 使い方
- 新しくレイヤーを追加します（名称は任意）。Main CameraのCulling Maskからはこのレイヤーのチェックを外します。
- Assets/Voxelizer/Prifabs/Voxelizerプレハブをシーンの任意の場所に配置します。
- Voxelizer配下のVoxelCameraのインスペクタを開き、CameraコンポーネントのCulling Maskを先ほど作成したレイヤーのみにチェックを入れた状態にします。
- Voxelizerのインスペクタを開き、ボクセル化のための各種設定を行います。
  - Mesh：ボクセルを指定したメッシュで置き換えます。なければ標準のCubeメッシュで構いません。
  - Center Pos：ボクセル化したい領域の中心座標を指定します。
  - Area Size：ボクセル化したい領域のサイズを指定します。Center Posを中心とした、一片の長さがArea Sizeの立方体がボクセル化対象の領域となります。
  - Grid Width：ボクセル化対象領域の1辺を何個のボクセルで分割するかを指定します。8の整数倍の数値を入力してください。
  - Target Pos：ここにTransformを設定すると、Center PosがそのTransfromに追従するようになります。
  - Height Scale：ボクセルの高さにスケールを掛けます。
  - FPS：ボクセルの更新間隔を指定できます。
- ボクセル化したいオブジェクトについて以下の設定を行います。
  - レイヤーを先ほど作成したレイヤーに変更します。
  - マテリアルのシェーダーをVoxelizerにします。必要に応じてテクスチャとベースカラーを設定します。
- 以上で、ボクセル化の対象領域内に配置されたオブジェクトが、ボクセル化されます。
## ライセンス
<div><img src=”http://unity-chan.com/images/imageLicenseLogo.png” alt=”ユニティちゃんライセンス”><p>Assets/UnityChan配下のデータは<a href=”http://unity-chan.com/contents/license_jp/” target=”_blank”>ユニティちゃんライセンス条項</a>の元に提供されています</p></div>
