from deeppavlov import build_model
from flask import Flask, request, jsonify
model = build_model('squad_ru_bert', download=True)

app = Flask(__name__)
@app.route('/api', methods=['POST'])
def api():
    data = request.get_json()
    if not all(k in data for k in ("entry", "question", "product")):
        return jsonify({"error": "Missing required data"}), 400
    gost = data.get('entry')
    question = data.get('question')
    product = data.get('product')
    print(question,product)
    try:
       result = model([gost],[product, question])
       print(result)
       return jsonify({"result":result})
    except Exception as e:
       return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=True)
