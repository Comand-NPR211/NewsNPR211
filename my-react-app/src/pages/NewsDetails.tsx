import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import Slider from "react-slick";
import axios from "axios";


interface NewsItem {
    id: number;
    title: string;
    description: string;
    content: string;
    categoryId: number;
    createdAt: string;
    updatedAt?: string;
    author: string;
    tags?: string;
    isPublished: boolean;
    views: number;
    source?: string;
    slug?: string;
    // imageUrl?: string;
    imageUrls?: string[]; //  Додано  поле
}

export default function NewsDetails() {
    const { id } = useParams();
    const [newsItem, setNewsItem] = useState<NewsItem | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        axios
            .get<NewsItem>(`http://localhost:5123/api/news/${id}`)
            .then((response) => {

                const data = response.data;
                if (typeof data.imageUrls === "string") {
                    data.imageUrls = JSON.parse(data.imageUrls);
                }



                setNewsItem(response.data);
            })
            .catch((error) => {
                console.error("Failed to fetch news:", error);
            })
            .finally(() => {
                setLoading(false);
            });
    }, [id]);

    if (loading) return <div className="text-center mt-10">Loading...</div>;
    if (!newsItem) return <div className="text-center mt-10 text-red-500">News not found</div>;

    return (
        <div className="max-w-3xl mx-auto p-4">
            {newsItem.imageUrls && newsItem.imageUrls.length > 0 && (
                <Slider dots={true} infinite={true} autoplay={true}>
                    {newsItem.imageUrls.map((imgUrl, index) => (
                        <img
                            key={index}
                            src={`http://localhost:5123/uploading/1200_${imgUrl}`}
                            alt={`news-image-${index}`}
                            className="rounded-xl mb-4 w-full max-h-[500px] object-contain mx-auto"
                        />
                    ))}
                </Slider>
            )}

            <h1 className="text-3xl font-bold mb-2">{newsItem.title}</h1>
            <p className="text-sm text-gray-500 mb-2">
                By <span className="font-medium">{newsItem.author}</span> | {new Date(newsItem.createdAt).toLocaleString()}
            </p>
            <p className="text-sm text-gray-500 mb-4">Views: {newsItem.views}</p>

            {newsItem.description && (
                <p className="text-lg italic mb-4 text-gray-700">{newsItem.description}</p>
            )}

            <div className="prose prose-lg max-w-none text-justify" dangerouslySetInnerHTML={{ __html: newsItem.content }} />

            {newsItem.tags && (
                <div className="mt-6">
                    <span className="text-sm text-gray-600">Tags: {newsItem.tags}</span>
                </div>
            )}

            {newsItem.source && (
                <div className="mt-4">
                    <a
                        href={newsItem.source}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 hover:underline"
                    >
                        Source
                    </a>
                </div>
            )}
        </div>
    );
}