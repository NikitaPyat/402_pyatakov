<template>
  <h3>Labels:</h3>
  <p>   
    <select @change="changeLabel($event)">
      <option v-if="labels.length === 0">empty</option>
      <option v-else v-for="label in labels" :key="label" >
        {{label}}
      </option>
    </select>
  </p>
  <img v-bind:src="currentImage" width="450" height="350" />
  <div>
    <button :onclick="next">next</button>
    <button :onclick="previous">previous</button>
  </div>
</template>

<script>

export default {
  data() {
    return {
      labels: [],
      currentLabel: "",
      images: [],
      currentImageNumber: -1,
      currentImage: "",
      picturesNumber: 0
    }
  }, 
  watch: {
    currentLabel(value) {
      this.images = [];
      this.currentImageNumber = -1;
      this.currentImage = "";
      fetch('http://localhost:5000/Pictures/labels/' + value)
        .then((response_images) => {
            return response_images.json();
        })
        .then((data) => {
            for (var j = 0; j < data.length; j++) {
                var im = document.createElement('img');
                im.src = "data:image/jpg;base64," + data[j];
                this.images.push(im.src);
            }
            this.picturesNumber = this.images.length;
            console.log(this.picturesNumber)
            this.currentImageNumber++;
        });
    },
    currentImageNumber(value) {
      this.currentImage = this.images[value];
      console.log(value);
    }
  },
  methods: {
    changeLabel(event) {
      this.currentLabel = event.target.value;
      console.log(this.currentLabel);
    },

    next() {
      console.log("next")
      if (this.currentImageNumber < this.picturesNumber - 1) {
        this.currentImageNumber++;
      }
    },

    previous() {
      if (this.currentImageNumber > 0) {
        this.currentImageNumber--;
      }
    }
  },
  beforeCreate() {
      fetch('http://localhost:5000/Pictures/labels').then((response) => {
        return response.json();
      })
      .then((data) => {
        this.labels = data;
      });
    }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
