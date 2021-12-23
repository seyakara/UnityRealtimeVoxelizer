# UnityRealtimeVoxelizer
![ezgif com-gif-maker(1)](https://user-images.githubusercontent.com/8469918/147265271-02da5d5b-8eb4-40d2-823d-6f1317b44370.gif)

Unity上で任意の3Dモデルをリアルタイムでボクセル化するシェーダーです。    
元のモデルのテクスチャおよびベースカラーをボクセル色に反映させることができます。  
また上記画像のようにボクセルを別のメッシュに置き換えて描画させることも可能です。  

## 環境
Unity 2021.2.5f1 URP

## 使い方
- ボクセル化したいオブジェクトに専用のレイヤーを割り当てます。MainCameraのCullingMaskからはこのレイヤーを除外してください。
- マテリアルのシェーダーに"Voxelizer"を指定します。必要に応じてテクスチャとベースカラーを指定します。
- プレハブ"Voxelizer"をシーン上に配置します。  
![スクリーンショット 2021-12-19 120303](https://user-images.githubusercontent.com/8469918/146662168-44a94e0b-1ff7-45a0-9cd6-71357419a07a.jpg)
  - Target Layer：ボクセル化したいオブジェクトのレイヤーを指定します。
  - Mesh：ボクセル１つをどのメッシュで描画するかを指定します。
  - Center Pos：ボクセル化したい領域の中心座標を指定します。
  - Area Size：ボクセル化したい領域のサイズを指定します。Center Posを中心とした、一片の長さがArea Sizeの立方体がボクセル化対象の領域となります。
  - Grid Width：ボクセル化対象領域の1辺を何個のボクセルで分割するかを指定します。8の整数倍の数値を入力してください。
  - Target Pos：ここにTransformを設定すると、Center PosがそのTransfromに追従するようになります。
  - Height Scale：ボクセルの高さにスケールを掛けます。
  - FPS：ボクセルの更新間隔を指定し、コマ送りのような効果を与えることができます。
## ライセンス
<div><img src=”http://unity-chan.com/images/imageLicenseLogo.png” alt=”ユニティちゃんライセンス”><p>Assets/UnityChan配下のデータは<a href=”http://unity-chan.com/contents/license_jp/” target=”_blank”>ユニティちゃんライセンス条項</a>の元に提供されています</p></div>
