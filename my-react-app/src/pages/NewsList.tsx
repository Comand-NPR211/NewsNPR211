
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import axios from "axios";

interface NewsItem {
    id: number;
    title: string;
    description: string;
    // imageUrl: string;       // нове поле з БД
    imageUrls?: string[]; //  оновлено: список зображень
    createdAt: string;
    categoryId: number;     // якщо знадобиться в майбутньому
}

export default function NewsList() {
    const [news, setNews] = useState<NewsItem[]>([]);

    useEffect(() => {
        axios
            .get("http://localhost:5123/api/news")

            .then((res) => {
                const newsArray = res.data || [];

                const parsedNews = (newsArray as NewsItem[]).map((item) => ({
                    ...item,
                    imageUrls: typeof item.imageUrls === "string"
                        ? JSON.parse(item.imageUrls)
                        : item.imageUrls ?? []
                }));

                setNews(parsedNews);
            })

            .catch((err) => {
                console.error("Error fetching news list", err);
                setNews([]);

            });
    }, []);

    return (
        <div className="p-4 max-w-4xl mx-auto">
            <h1 className="text-3xl font-bold mb-4">📰 Список новин</h1>

            {news.length > 0 ? (
                news.map((item) => (

                    // <div key={item.id} className="border-b border-gray-300 py-4 flex gap-4">
                    <div key={item.id} className="border-b border-gray-300 py-4 flex flex-col gap-2">

                        <Link
                            to={`/news/${item.id}`}
                            className="text-xl text-blue-600 hover:underline"
                        >
                            {item.title}
                        </Link>
                        <p className="text-sm text-gray-600">
                            {new Date(item.createdAt).toLocaleDateString()}
                        </p>
                        <p className="text-gray-700 mt-1">{item.description}</p>

                        {/* ✅ НОВЕ: блок прев’ю з кількох зображень */}
                        <div className="flex gap-2 mt-2 overflow-x-auto">
                            {item.imageUrls?.slice(0, 3).map((img, index) => (
                                <img
                                    key={index}
                                    src={`http://localhost:5123/uploading/150_${img}`} // ✅ Зміна префіксу
                                    alt={`preview-${index}`}
                                    className="w-24 h-16 object-cover rounded"
                                />
                            ))}

                        </div>
                    </div>
                ))
            ) : (
                <p>🔍 Новини не знайдені або помилка під час завантаження.</p>
            )}
        </div>
    );
}