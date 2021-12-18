# UnityRealtimeVoxelizer
![20211219_005035_Moment2](https://user-images.githubusercontent.com/8469918/146647348-4952824c-7580-4374-be3c-d198507f4ca9.jpg)  
Unity上で任意の3Dモデルをリアルタイムでボクセル化して描画できるシェーダーです。    
元のモデルのテクスチャおよびベースカラーをボクセル色に反映させることができます。  
また上記画像のようにボクセルを別のメッシュに置き換えて描画させることも可能です。


## 使い方
- 新しくレイヤーを追加します（名称は任意）。Main CameraのCulling Maskからはこのレイヤーのチェックを外してください。
- Assets/Voxelizer/Prifabs/Voxelizerプレハブをシーンの任意の場所に配置します。
- Voxelizer配下のVoxelCameraのインスペクタを開き、CameraコンポーネントのCulling Maskを先ほど作成したレイヤーのみにチェックを入れた状態にします。
- Voxelizerのインスペクタを開き、各種設定を行います。
  - Mesh：ボクセル１つをどのメッシュで描画するかを指定します。標準のCubeメッシュでも構いません。
  - Center Pos：ボクセル化したい領域の中心座標を指定します。
  - Area Size：ボクセル化したい領域のサイズを指定します。Center Posを中心とした、一片の長さがArea Sizeの立方体がボクセル化対象の領域となります。
  - Grid Width：ボクセル化対象領域の1辺を何個のボクセルで分割するかを指定します。8の整数倍の数値を入力してください。
  - Target Pos：ここにTransformを設定すると、Center PosがそのTransfromに追従するようになります。
  - Height Scale：ボクセルの高さにスケールを掛けます。
  - FPS：ボクセルの更新間隔を指定し、コマ送りのような効果を与えることができます。
- ボクセル化したいオブジェクトについて以下の設定を行います。
  - レイヤーを先ほど作成したレイヤーに変更します。
  - マテリアルのシェーダーをVoxelizerにします。必要に応じてテクスチャとベースカラーを設定します。
- 以上で、ボクセル化の対象領域内に配置されたオブジェクトが、ボクセル化されます。
## ライセンス
<div><img src=”http://unity-chan.com/images/imageLicenseLogo.png” alt=”ユニティちゃんライセンス”><p>Assets/UnityChan配下のデータは<a href=”http://unity-chan.com/contents/license_jp/” target=”_blank”>ユニティちゃんライセンス条項</a>の元に提供されています</p></div>
