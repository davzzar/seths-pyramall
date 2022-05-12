<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.8" tiledversion="1.8.2" name="Halo" tilewidth="32" tileheight="32" tilecount="8" columns="8">
 <image source="../TiledsetTexture/HardJumpHalo.png" width="256" height="32"/>
 <tile id="0">
  <properties>
   <property name="AnimIsEntry" type="bool" value="true"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="CanHardJump"/>
  </properties>
  <animation>
   <frame tileid="0" duration="60"/>
   <frame tileid="1" duration="60"/>
   <frame tileid="2" duration="60"/>
  </animation>
 </tile>
 <tile id="3">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="HardJumpPressed"/>
  </properties>
  <animation>
   <frame tileid="3" duration="30"/>
   <frame tileid="4" duration="30"/>
   <frame tileid="5" duration="30"/>
  </animation>
 </tile>
 <tile id="6">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Idle"/>
  </properties>
  <animation>
   <frame tileid="6" duration="300"/>
  </animation>
 </tile>
</tileset>
