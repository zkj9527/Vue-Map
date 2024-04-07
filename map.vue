<template>
    <div id="map" />
    <div id="popup" ref="mapContent" v-html="mapText"></div>
  </template>
  
  <script setup>
  import 'ol/ol.css'
  import TileLayer from 'ol/layer/Tile'
  import XYZ from 'ol/source/XYZ'
  import VectorLayer from 'ol/layer/Vector'
  import VectorSource from 'ol/source/Vector'
  import { Map, View,Feature,Overlay} from 'ol'
  import { Point as olPoint } from 'ol/geom'
  import { fromLonLat,transform} from 'ol/proj'
  import { Style, Fill,Text,Icon } from 'ol/style'   //Icon
  import { ref, reactive, onMounted,defineProps,defineExpose } from 'vue'
  
  const map = ref(null)
  // 弹框
  const overlay = ref(null)
  const mapContent = ref(null)
  const mapText = ref(null)
  
  const mapView = reactive({
    center: fromLonLat([121.473667,31.230525]), // 地图中心点
    zoom: 7, // 初始缩放级别
    minZoom:3, // 最小缩放级别
    maxZoom: 8, // 最大缩放级别
  })
  const mapUrl = `tiles/{z}/{x}/{y}.png`
  
  onMounted(() => {
    initMap()
  })
  // 父组件传参
  const props = defineProps({
    location:{
      type:Object,
      required:true
    }
  })
  
  // 创建点位数组
  /**const locations = [
    {
      name:'上海',
      title:'',
      long:[121.473667,31.230525]
    },
    {
      name:'北京',
      title:'',
      long:[116.407387,39.904179]
    },
    {
      name:'深圳',
      title:'',
      long:[114.057939,22.543527]
    },
    ]*/
  
  const initMap = () => {
    const tileLayer = new TileLayer({
      source: new XYZ({
        url: mapUrl
      })
    })
    map.value = null
    map.value = new Map({
      layers: [tileLayer],
      view: new View(mapView),
      target: 'map' // 将地图注入到 dom 元素中，此处写 dom id
    })
    
    
    /** 循环添加点位
    props.location.forEach((v)=>{
      addLayer(v)
    })*/
  
    // 初始化地图之后就将弹框挂载好，后续只是修改显示的位置
    createOverlay()
    // 地图点击
    map.value.on('click', (e) => mapClick(e))
  }
  // 添加点位
  const addLayer = (v) => {
    const layer = new VectorLayer({
        source: new VectorSource()
    })
    // 添加图层
    map.value.addLayer(layer)
    // 创建 feature 坐标信息
    const feature = new Feature({
        // 经纬度转换成坐标信息
        geometry: new olPoint(fromLonLat(v.long)),
        // 这里将所有的参数都放进来，供后续使用
        ...v
    })
    feature.setStyle(
        new Style({
            // 标点的图片，如果要标不同类型的点，这个图片可以判断加
            image: new Icon({
                crossOrigin: 'anonymous',
                src: new URL('@/assets/images/地图标点.png',
                    import.meta.url).href
            }),
            // 标点的文字
            text: new Text({
                // 文字
                text: v.name,
                // 文字样式
                fill: new Fill({
                    color: 'white'
                }),
                backgroundFill: new Fill({
                    color: 'black'
                }),
                font: '10px Calibri',
                // 偏移量
                offsetY: 20
            })
        })
    )
    // 将 feature 坐标信息添加在地图上
    layer.getSource().addFeatures([feature])
  }
  
  // 创建弹框
  const createOverlay = () => {
    overlay.value = new Overlay({
      element: mapContent.value, // 将弹框挂载在 dom 上
      autoPan: true, // 如果弹框显示不全则自动归位
      positioning: 'bottom-center', // 相对于其位置属性的实际位置
      stopEvent: true, // 事件冒泡
      autoPanAnimation: {
          duration: 300 // 地图移动速度
      },
      offset:[0,-20]
    })
  
    map.value.addOverlay(overlay.value) // 将弹框添加到地图上
  }
  
  // 关闭弹框
  const closeMapPopup = () => {
      overlay.value.setPosition(undefined)
  }
  
  // 地图点击
  const mapClick = (e) => {
      const lonlat = transform(e.coordinate, 'EPSG:3857', 'EPSG:4326')  // eslint-disable-line no-unused-vars 
      // 判断当前点击是否点击在图标上
      const feature = map.value.forEachFeatureAtPixel(e.pixel, (feature) => feature)
      if (feature) {
          // 弹框内容
          mapText.value = `<p>${feature.values_.name}<p><p>${feature.values_.title}<p>`
          // 把 overlay 显示到指定的坐标位置
          overlay.value.setPosition(fromLonLat(feature.values_.long))
      } else {
          // 弹框关闭
          closeMapPopup()
      }
  }
  // 暴露方法
  defineExpose({ addLayer,initMap })
  
  </script>
  
  <style scoped>
  #map {
      height: 200px;
          width: 400px;
      display: flex;
      align-items: center;
  }
  
  #popup {
      background-color: #fff;
      filter: drop-shadow(0 1px 4px rgba(0, 0, 0, 0.2));
      padding: 15px;
      border-radius: 10px;
      border: 1px solid #cccccc;
      top: 0;
      left: 0;
  }
  </style>